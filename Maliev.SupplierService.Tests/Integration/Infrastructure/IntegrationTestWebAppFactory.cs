using System.Security.Claims;
using Maliev.SupplierService.Application.Interfaces;
using Maliev.SupplierService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using Xunit;

[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]

namespace Maliev.SupplierService.Tests.Integration.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly RedisContainer _redisContainer;
    private readonly RabbitMqContainer _rabbitMqContainer;

    private string? _connectionString;
    private string? _redisConnectionString;
    private string? _rabbitMqConnectionString;

    public IntegrationTestWebAppFactory()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:18-alpine")
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:8.4-alpine")
            .Build();

        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:4.2-alpine")
            .Build();

        _postgresContainer.StartAsync().GetAwaiter().GetResult();
        _redisContainer.StartAsync().GetAwaiter().GetResult();
        _rabbitMqContainer.StartAsync().GetAwaiter().GetResult();

        _connectionString = _postgresContainer.GetConnectionString();
        _redisConnectionString = _redisContainer.GetConnectionString();
        _rabbitMqConnectionString = _rabbitMqContainer.GetConnectionString();
    }

    public string ConnectionString => _connectionString ?? throw new InvalidOperationException("Connection string not initialized");

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return DisposeAsync().AsTask();
    }

    public override async ValueTask DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
        await _rabbitMqContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:SupplierDbContext"] = ConnectionString,
                ["ConnectionStrings:redis"] = _redisConnectionString,
                ["ConnectionStrings:rabbitmq"] = _rabbitMqConnectionString,
                ["CORS:AllowedOrigins:0"] = "http://localhost:5000"
            })
            .Build();

        builder.UseConfiguration(config);

        builder.ConfigureTestServices(services =>
        {
            // Remove existing DbContext to replace with test configuration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SupplierDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<SupplierDbContext>(options =>
            {
                options.UseNpgsql(ConnectionString);
            });

            // Ensure database is created (no migrations exist)
            using var scope = services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SupplierDbContext>();
            context.Database.EnsureCreated();

            // Use TestAuthHandler for authentication in tests
            services.AddAuthentication(TestAuthHandler.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.AuthenticationScheme, options => { });

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

    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
        return client;
    }

    public HttpClient CreatePermissionAuthenticatedClient(string userId = "test-user", string[]? permissions = null)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
        return client;
    }
}

[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>
{
}
