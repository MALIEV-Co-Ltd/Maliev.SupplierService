#pragma warning disable CA1848 // For improved performance, use the LoggerMessage delegates
using Maliev.Aspire.ServiceDefaults;
using Maliev.SupplierService.Api.Extensions;
using Maliev.SupplierService.Api.Services;
using Maliev.SupplierService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.RateLimiting;

// Initialize bootstrap logging
using var loggerFactory = LoggerFactory.Create(logBuilder => logBuilder.AddConsole());
var bootstrapLogger = loggerFactory.CreateLogger("Program");

try
{
    bootstrapLogger.LogInformation("Starting Supplier Service host");

    var builder = WebApplication.CreateBuilder(args);

    // --- Secrets & Configuration ---
    builder.AddGoogleSecretManagerVolume(); // Load secrets from /mnt/secrets if available

    // --- Infrastructure & Observability ---
    builder.AddServiceDefaults(); // OpenTelemetry, health checks, resilience
    builder.AddStandardMiddleware(options =>
    {
        options.EnableRequestLogging = true;
    });
    builder.AddServiceMeters("suppliers-meter"); // Register service meters for OpenTelemetry business metrics

    builder.AddPostgresDbContext<Maliev.SupplierService.Data.SupplierDbContext>(
        configureOptions: options =>
        {
            if (builder.Environment.IsEnvironment("Testing"))
            {
                options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            }
        },
        connectionName: "SupplierDbContext"); // PostgreSQL with retry logic
    builder.AddRedisDistributedCache(instanceName: "supplier:"); // Redis with in-memory fallback
    builder.AddMassTransitWithRabbitMq(cfg =>
    {
        cfg.AddEntityFrameworkOutbox<SupplierDbContext>(o =>
        {
            o.UsePostgres();
            o.UseBusOutbox();
        });
    }); // RabbitMQ message bus (non-blocking startup)

    // --- API Configuration ---
    builder.AddDefaultCors(); // CORS from CORS:AllowedOrigins config
    builder.AddDefaultApiVersioning(); // API versioning with URL segment reader

    // JWT Authentication (tests override via PostConfigureAll with dynamic RSA keys)
    builder.AddJwtAuthentication();

    // --- Authorization ---
    builder.Services.AddPermissionAuthorization();

    // IAM Registration
    builder.AddIAMServiceClient("supplier");
    builder.Services.AddIAMRegistration<SupplierIAMRegistrationService>("supplier");

    // Add OpenAPI (must be in Program.cs for XML comments to work via source generator)
    if (!builder.Environment.IsProduction())
    {
        builder.AddStandardOpenApi(
            title: "MALIEV Supplier Service API",
            description: "Supplier relationship management service. Manages supplier registration and onboarding, contact information, certification tracking with expiry alerts, performance evaluations, eligibility checks for purchase orders, and status management (active/inactive/suspended).");
    }

    // Add services
    builder.Services.AddSupplierServices(builder.Configuration);
    builder.Services.AddExternalServiceClients(builder.Configuration);

    // Add controllers
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });
    builder.Services.AddOpenApi();

    // Add rate limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
            context => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 10
                }));
    });

    var app = builder.Build();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    // --- Database Migrations ---
    await app.MigrateDatabaseAsync<SupplierDbContext>();

    // Use custom middleware
    app.UseStandardMiddleware();

    // Middleware Pipeline
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
    app.UseCors();
    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();

    // Map controllers
    app.MapControllers();

    // Map Aspire health endpoints
    app.MapDefaultEndpoints(servicePrefix: "supplier");

    // Map OpenAPI and Scalar documentation (dev/staging only)
    app.MapApiDocumentation(servicePrefix: "supplier");

    logger.LogInformation("SupplierService started successfully");
    await app.RunAsync();
}
catch (Exception ex)
{
    bootstrapLogger.LogCritical(ex, "Supplier Service host terminated unexpectedly during startup");
    throw;
}
finally
{
    loggerFactory.Dispose();
}

/// <summary>
/// Main program class for the Supplier Service API.
/// </summary>
public partial class Program { }
