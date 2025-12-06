using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Maliev.SupplierService.Api.Services;
using Maliev.SupplierService.Data;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace Maliev.SupplierService.Tests.Integration.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:18")
        .WithDatabase("supplier_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management-alpine")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();

    private readonly RSA _testRsa;
    private const string TestIssuer = "test-issuer";
    private const string TestAudience = "test-audience";

    public IntegrationTestWebAppFactory()
    {
        // Generate ephemeral RSA key for test JWT tokens
        _testRsa = RSA.Create(2048);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove existing DbContext and factory registrations
            services.RemoveAll<DbContextOptions<SupplierDbContext>>();
            services.RemoveAll<SupplierDbContext>();
            services.RemoveAll<IDbContextFactory<SupplierDbContext>>();

            // Add test database and factory
            services.AddDbContextFactory<SupplierDbContext>(options =>
            {
                options.UseNpgsql(_postgresContainer.GetConnectionString());
            });
            services.AddDbContext<SupplierDbContext>(options =>
            {
                options.UseNpgsql(_postgresContainer.GetConnectionString());
            });

            // Replace Redis
            services.RemoveAll<IConnectionMultiplexer>();
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString()));

            // Replace cache service with test implementation
            services.RemoveAll<ICacheService>();
            services.AddScoped<ICacheService, TestCacheService>();

            // Remove ALL MassTransit-related services to avoid duplicates
            var massTransitDescriptors = services.Where(d =>
                d.ServiceType.FullName?.Contains("MassTransit") == true ||
                d.ImplementationType?.FullName?.Contains("MassTransit") == true).ToList();

            foreach (var descriptor in massTransitDescriptors)
            {
                services.Remove(descriptor);
            }

            // Remove hosted services that are MassTransit-related
            var hostedServiceDescriptors = services.Where(d =>
                d.ServiceType == typeof(IHostedService) &&
                d.ImplementationType?.FullName?.Contains("MassTransit") == true).ToList();

            foreach (var descriptor in hostedServiceDescriptors)
            {
                services.Remove(descriptor);
            }

            // Clear all health check registrations to avoid duplicates
            services.Configure<HealthCheckServiceOptions>(options =>
            {
                options.Registrations.Clear();
            });

            // Configure MassTransit for testing (in-memory)
            services.AddMassTransitTestHarness(cfg =>
            {
                cfg.SetTestTimeouts(testInactivityTimeout: TimeSpan.FromSeconds(5));
            });

            // PostConfigure JWT Bearer options to use our test RSA key
            services.PostConfigureAll<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = TestIssuer,
                    ValidAudience = TestAudience,
                    IssuerSigningKey = new RsaSecurityKey(_testRsa),
                    ClockSkew = TimeSpan.Zero // No clock skew for tests
                };
            });
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await _redisContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();

        // Create database schema from model
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SupplierDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.StopAsync();
        await _redisContainer.StopAsync();
        await _rabbitMqContainer.StopAsync();

        await _postgresContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
        await _rabbitMqContainer.DisposeAsync();

        _testRsa.Dispose();
        await base.DisposeAsync();
    }

    /// <summary>
    /// Creates a test JWT token with specified claims for integration testing.
    /// </summary>
    /// <param name="userId">User ID claim</param>
    /// <param name="roles">User roles</param>
    /// <param name="additionalClaims">Additional claims to include</param>
    /// <returns>JWT token string</returns>
    public string CreateTestJwtToken(string userId = "test-user", string[]? roles = null, Dictionary<string, string>? additionalClaims = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add roles
        roles ??= new[] { "Admin" };
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add additional claims
        if (additionalClaims != null)
        {
            foreach (var (key, value) in additionalClaims)
            {
                claims.Add(new Claim(key, value));
            }
        }

        var credentials = new SigningCredentials(
            new RsaSecurityKey(_testRsa),
            SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

/// <summary>
/// Simple in-memory cache service for testing
/// </summary>
public class TestCacheService : ICacheService
{
    private readonly Dictionary<string, object> _cache = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var value) && value is T typedValue)
        {
            return Task.FromResult<T?>(typedValue);
        }
        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        _cache[key] = value!;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task InvalidateByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        // For testing, clear all cache entries
        _cache.Clear();
        return Task.CompletedTask;
    }
}
