using Maliev.SupplierService.Api.Extensions;
using Prometheus;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add Aspire ServiceDefaults
builder.AddServiceDefaults();

// Add services
builder.Services.AddSupplierServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddRedisCache(builder.Configuration);
builder.Services.AddMassTransitWithRabbitMq(builder.Configuration);
builder.Services.AddApiVersioningConfiguration();
builder.Services.AddExternalServiceClients(builder.Configuration);

// Add OpenAPI
builder.Services.AddOpenApi();

// Add controllers
builder.Services.AddControllers();

// Add rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(
        context => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 10
            }));
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() ?? [];
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader();
        }
    });
});

var app = builder.Build();

// Use custom middleware
app.UseSupplierServiceMiddleware();

// Use Serilog request logging
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Map OpenAPI at /suppliers/openapi/v1.json
    app.MapOpenApi("/suppliers/openapi/{documentName}.json");

    // Map Scalar at /suppliers/scalar/v1 path (matches ingress /suppliers prefix)
    app.MapScalarApiReference("/suppliers/scalar/v1", options =>
    {
        options
            .WithTitle("MALIEV Supplier Service API")
            .WithTheme(ScalarTheme.Default)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
            .WithOpenApiRoutePattern("/suppliers/openapi/v1.json");
    });

    // Redirect root to Scalar
    app.MapGet("/", () => Results.Redirect("/suppliers/scalar/v1")).ExcludeFromDescription();
    app.MapGet("/suppliers", () => Results.Redirect("/suppliers/scalar/v1")).ExcludeFromDescription();
}

// Use CORS
app.UseCors();

// Use rate limiting
app.UseRateLimiter();

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map Aspire health endpoints
app.MapDefaultEndpoints();

// Map Prometheus metrics
app.MapMetrics("/suppliers/metrics");

// Map health check endpoints
app.MapHealthChecks("/suppliers/liveness", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // No checks for liveness
});

app.MapHealthChecks("/suppliers/readiness", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

try
{
    Log.Information("Starting Supplier Service API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
