using System.Text;

using Maliev.SupplierService.Api.Configuration;
using Maliev.SupplierService.Api.Services;
using Maliev.SupplierService.Api.Services.ExternalServices;
using Maliev.SupplierService.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace Maliev.SupplierService.Api.Extensions;

/// <summary>
/// Provides extension methods for configuring services in the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds core supplier-related services, configuration, database contexts, validators, and business services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The application's configuration.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddSupplierServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuration
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<RedisSettings>(configuration.GetSection(RedisSettings.SectionName));
        services.Configure<RabbitMQSettings>(configuration.GetSection(RabbitMQSettings.SectionName));

        // Note: Database is now configured via builder.AddPostgresDbContext<SupplierDbContext>() in Program.cs



        // Services
        services.AddScoped<ISupplierService, Services.SupplierService>();
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }

    /// <summary>
    /// Adds JWT authentication to the service collection, configuring JWT bearer options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The application's configuration.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtPublicKey = configuration["Jwt:PublicKey"] ?? throw new InvalidOperationException("Jwt:PublicKey not found");
        var jwtIssuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer not found");
        var jwtAudience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience not found");

        // Parse RSA public key
        var rsa = System.Security.Cryptography.RSA.Create();
        try
        {
            var publicKeyPem = Encoding.UTF8.GetString(Convert.FromBase64String(jwtPublicKey));
            rsa.ImportFromPem(publicKeyPem);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Invalid JWT public key format", ex);
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new RsaSecurityKey(rsa),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Adds and configures API versioning, including API explorer support for documentation generation.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    /// <summary>
    /// Adds HTTP clients for external services with configured base addresses, timeouts, and standard resilience policies.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The application's configuration.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddExternalServiceClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Use standardized helpers from ServiceDefaults
        services.AddServiceClient<IPurchaseOrderServiceClient, PurchaseOrderServiceClient>(configuration, "PurchaseOrderService");
        services.AddServiceClient<IInvoiceServiceClient, InvoiceServiceClient>(configuration, "InvoiceService");
        services.AddServiceClient<IMaterialServiceClient, MaterialServiceClient>(configuration, "MaterialService");

        return services;
    }
}
