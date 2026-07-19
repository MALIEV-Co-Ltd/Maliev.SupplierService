using Maliev.Aspire.ServiceDefaults;
using Maliev.SupplierService.Api.Extensions;
using Maliev.SupplierService.Api.Services;
using Maliev.SupplierService.Application;
using Maliev.SupplierService.Infrastructure;
using Maliev.SupplierService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

// Initialize bootstrap logging
using var loggerFactory = LoggerFactory.Create(logBuilder => logBuilder.AddConsole());
var bootstrapLogger = loggerFactory.CreateLogger("Program");

try
{
    Log.StartingHost(bootstrapLogger, "Supplier Service");

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

    builder.AddPostgresDbContext<SupplierDbContext>(
        configureOptions: options =>
        {
            if (builder.Environment.IsEnvironment("Testing"))
            {
                options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            }
        },
        connectionName: "SupplierDbContext"); // PostgreSQL with retry logic

    builder.AddStandardCache("supplier:"); // Redis + in-memory fallback, memory-optimized

    // Configure MassTransit - skip outbox in Testing environment
    if (!builder.Environment.IsEnvironment("Testing"))
    {
        builder.AddMassTransitWithRabbitMq(cfg =>
        {
            cfg.AddEntityFrameworkOutbox<SupplierDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });
        });
    }
    else
    {
        builder.AddMassTransitWithRabbitMq();
    } // RabbitMQ message bus (non-blocking startup)

    // --- API Configuration ---
    builder.AddStandardCors(); // CORS with fail-fast validation
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

    // --- Layer Registration ---
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Add controllers
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    // Add rate limiting
    builder.AddStandardRateLimiting(); // Memory-optimized for low-spec nodes

    var app = builder.Build();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    // --- Database Schema ---
    if (app.Environment.IsEnvironment("Testing"))
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SupplierDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }
    else
    {
        await app.MigrateDatabaseAsync<SupplierDbContext>();
    }

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

    Log.ServiceStarted(logger, "Supplier Service");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.HostTerminated(bootstrapLogger, ex, "Supplier Service");
    throw;
}
finally
{
    loggerFactory.Dispose();
}

/// <summary>
/// Main program class for the Supplier Service API.
/// </summary>
public partial class Program
{
    internal static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Starting {ServiceName} host")]
        public static partial void StartingHost(ILogger logger, string serviceName);

        [LoggerMessage(Level = LogLevel.Critical, Message = "{ServiceName} host terminated unexpectedly during startup")]
        public static partial void HostTerminated(ILogger logger, Exception ex, string serviceName);

        [LoggerMessage(Level = LogLevel.Information, Message = "{ServiceName} started successfully")]
        public static partial void ServiceStarted(ILogger logger, string serviceName);
    }
}
