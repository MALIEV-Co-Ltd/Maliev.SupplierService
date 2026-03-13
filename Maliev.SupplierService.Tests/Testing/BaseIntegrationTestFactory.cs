using System.IdentityModel.Tokens.Jwt;
using System.Diagnostics.CodeAnalysis;
using MassTransit;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using Xunit;
using NSubstitute;
using Maliev.SupplierService.Application.Interfaces;

namespace Maliev.SupplierService.Tests.Testing;

public class BaseIntegrationTestFactory<TProgram, TDbContext> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class
    where TDbContext : DbContext
{
    private static PostgreSqlContainer? _postgresContainer;
    private static RedisContainer? _redisContainer;
    private static RabbitMqContainer? _rabbitmqContainer;
    private static bool _containersStarted;
    private static readonly SemaphoreSlim _initLock = new(1, 1);

    private readonly RSA _testRsa;

    protected virtual string DbConnectionStringName => typeof(TDbContext).Name;

    public BaseIntegrationTestFactory()
    {
        _testRsa = RSA.Create(2048);
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
    }

    public async Task InitializeAsync()
    {
        await _initLock.WaitAsync();
        try
        {
            if (!_containersStarted)
            {
                _postgresContainer =
#pragma warning disable CS0618
        new PostgreSqlBuilder().WithImage("postgres:18-alpine").Build();
                _redisContainer = new RedisBuilder().WithImage("redis:8.4-alpine").Build();
                _rabbitmqContainer = new RabbitMqBuilder().WithImage("rabbitmq:4.2-alpine").Build();
#pragma warning restore CS0618

                await Task.WhenAll(
                    _postgresContainer.StartAsync(),
                    _redisContainer.StartAsync(),
                    _rabbitmqContainer.StartAsync()
                );

                await ApplyMigrationsAsync();
                _containersStarted = true;
            }
        }
        finally
        {
            _initLock.Release();
        }

        Environment.SetEnvironmentVariable($"ConnectionStrings__{DbConnectionStringName}", _postgresContainer!.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__redis", _redisContainer!.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__rabbitmq", _rabbitmqContainer!.GetConnectionString());
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        _testRsa.Dispose();
    }

    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var rsaParams = _testRsa.ExportParameters(false);
        var publicKeyPem = _testRsa.ExportRSAPublicKeyPem();
        var publicKeyBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(publicKeyPem));
        Environment.SetEnvironmentVariable("Jwt__PublicKey", publicKeyBase64);

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.PostConfigureAll<DbContextOptions<TDbContext>>(options =>
            {
                var builder = new DbContextOptionsBuilder<TDbContext>(options);
                builder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });

            services.PostConfigureAll<JwtBearerOptions>(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "test-issuer",
                    ValidAudience = "test-audience",
                    IssuerSigningKey = new RsaSecurityKey(_testRsa)
                };
            });

            var poClient = Substitute.For<IPurchaseOrderServiceClient>();
            poClient.CheckReferencesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult((false, false)));
            var invoiceClient = Substitute.For<IInvoiceServiceClient>();
            invoiceClient.CheckReferencesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult((false, false)));
            var materialClient = Substitute.For<IMaterialServiceClient>();
            materialClient.CheckReferencesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult((false, false)));

            services.AddSingleton(poClient);
            services.AddSingleton(invoiceClient);
            services.AddSingleton(materialClient);
        });
    }

    public TDbContext CreateDbContext()
    {
        var connectionString = _postgresContainer!.GetConnectionString();
        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return (TDbContext)Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options)!;
    }

    private async Task ApplyMigrationsAsync()
    {
        await using var context = CreateDbContext();
        await context.Database.MigrateAsync();
    }

    public string CreateTestJwtToken(string userId = "test-user", string[]? permissions = null)
    {
        var claims = new List<Claim> { new(JwtRegisteredClaimNames.Sub, userId) };
        if (permissions != null) claims.AddRange(permissions.Select(p => new Claim("permissions", p)));

        var token = new JwtSecurityToken("test-issuer", "test-audience", claims, expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(new RsaSecurityKey(_testRsa), SecurityAlgorithms.RsaSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public HttpClient CreatePermissionAuthenticatedClient(string userId = "test-user", string[]? permissions = null)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {CreateTestJwtToken(userId, permissions)}");
        return client;
    }
}
