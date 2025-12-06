using Maliev.SupplierService.Api.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// --- Secrets & Configuration ---
builder.AddGoogleSecretManagerVolume(); // Load secrets from /mnt/secrets if available

// --- Infrastructure & Observability ---
builder.AddServiceDefaults(); // OpenTelemetry, health checks, resilience
builder.AddServiceMeters("suppliers"); // Register service meters for OpenTelemetry business metrics

builder.AddPostgresDbContext<Maliev.SupplierService.Data.SupplierDbContext>(connectionStringName: "SupplierDbContext"); // PostgreSQL with retry logic
builder.AddRedisDistributedCache(instanceName: "Supplier:"); // Redis with in-memory fallback
builder.AddMassTransitWithRabbitMq(); // RabbitMQ message bus (non-blocking startup)

// Add DbContextFactory for AuditService (skip in Testing environment - tests manually register)
if (!builder.Environment.IsEnvironment("Testing"))
{
    var connectionString = builder.Configuration.GetConnectionString("SupplierDbContext")
        ?? throw new InvalidOperationException("Database connection string not found. Expected 'ConnectionStrings:SupplierDbContext'");

    builder.Services.AddDbContextFactory<Maliev.SupplierService.Data.SupplierDbContext>(options =>
    {
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null);
        });
    }, Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped);
}

// --- API Configuration ---
builder.AddDefaultCors(); // CORS from CORS:AllowedOrigins config
builder.AddDefaultApiVersioning(); // API versioning with URL segment reader

// JWT Authentication (tests override via PostConfigureAll with dynamic RSA keys)
builder.AddJwtAuthentication();

// Add OpenAPI (must be in Program.cs for XML comments to work via source generator)
if (!builder.Environment.IsProduction())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi("v1", options =>
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Info.Title = "Supplier Service API";
            document.Info.Version = "v1";
            document.Info.Description = "Supplier relationship management service. Manages supplier registration and onboarding, contact information, certification tracking with expiry alerts, performance evaluations, eligibility checks for purchase orders, and status management (active/inactive/suspended).";
            return Task.CompletedTask;
        });
    });
}

// Add services
builder.Services.AddSupplierServices(builder.Configuration);
builder.Services.AddExternalServiceClients(builder.Configuration);

// Add controllers
builder.Services.AddControllers();

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

// Run database migrations on startup (skip in Testing environment)
if (!app.Environment.IsEnvironment("Testing"))
{
    try
    {
        await app.MigrateDatabaseAsync<Maliev.SupplierService.Data.SupplierDbContext>();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration failed - application may not function correctly");
        // Don't throw - allow app to start for debugging
    }
}

// Use custom middleware
app.UseSupplierServiceMiddleware();

// Middleware Pipeline
app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map Aspire health endpoints
app.MapDefaultEndpoints(servicePrefix: "suppliers");

// Map OpenAPI and Scalar documentation (dev/staging only)
app.MapApiDocumentation(servicePrefix: "suppliers");

logger.LogInformation("SupplierService started successfully");
await app.RunAsync();

/// <summary>
/// Main program class for the Supplier Service API.
/// </summary>
public partial class Program { }
