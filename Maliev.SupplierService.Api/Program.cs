using Asp.Versioning;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Maliev.SupplierService.Data.DbContexts;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Serilog Configuration (Console only)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Secrets from Google Secret Manager
var secretsPath = "/mnt/secrets";
if (Directory.Exists(secretsPath))
{
    builder.Configuration.AddKeyPerFile(directoryPath: secretsPath, optional: true);
}

// Database
var connectionString = builder.Configuration.GetConnectionString("SupplierDbContext")
    ?? builder.Configuration["SupplierDbContext"];

if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<SupplierDbContext>(options =>
        options.UseNpgsql(connectionString));
}

// Services
builder.Services.AddControllers();
builder.Services.AddMemoryCache();

// API Versioning
builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(1, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new QueryStringApiVersionReader("version"),
        new HeaderApiVersionReader("X-Version"));
}).AddApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

// OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<SupplierDbContext>(tags: new[] { "readiness" });

// Authentication
if (!builder.Environment.IsEnvironment("Testing"))
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"] ?? builder.Configuration["JwtSecretKey"];

    if (!string.IsNullOrEmpty(secretKey))
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"] ?? builder.Configuration["JwtIssuer"],
                    ValidAudience = jwtSettings["Audience"] ?? builder.Configuration["JwtAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });
    }
    else
    {
        Log.Warning("JWT settings not configured - authentication disabled");
    }
}

var app = builder.Build();

// Middleware Pipeline (EXACT ORDER)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "suppliers/swagger";
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Supplier Service API v1");
});

app.UseHttpsRedirection();

app.UseRouting();

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

// Health checks
app.MapGet("/suppliers/liveness", () => "Healthy").AllowAnonymous();
app.MapHealthChecks("/suppliers/readiness", new HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Tags.Contains("readiness"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).AllowAnonymous();

app.MapControllers();

app.Run();