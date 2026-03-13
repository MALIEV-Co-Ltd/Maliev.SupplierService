using System.Security.Claims;
using DotNet.Testcontainers.Builders;
using Maliev.Aspire.ServiceDefaults.IAM;
using Maliev.SupplierService.Application.Interfaces;
using Maliev.SupplierService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using Xunit;

namespace Maliev.SupplierService.Tests.Integration.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly RedisContainer _redisContainer;
    private readonly RabbitMqContainer _rabbitMqContainer;

    private string? _connectionString;
    private string? _redisConnectionString;
    private string? _rabbitMqConnectionString;

    // Phase 3: Transaction support
    private IDbContextTransaction? _currentTransaction;

    public IntegrationTestWebAppFactory()
    {
        // Phase 2: Optimized container configuration
        _postgresContainer =
#pragma warning disable CS0618
        new PostgreSqlBuilder().WithImage("postgres:18-alpine")
            .WithDatabase("supplier_test")
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:8.4-alpine")
            .Build();

        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:4.2-alpine")
            .Build();

    }

    public string ConnectionString => _connectionString ?? throw new InvalidOperationException("Connection string not initialized");

    // Phase 3: Transaction support methods
    public IDbContextTransaction? CurrentTransaction => _currentTransaction;

    public async Task<IDbContextTransaction> BeginTransactionAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<SupplierDbContext>();
        _currentTransaction = await context.Database.BeginTransactionAsync();
        return _currentTransaction;
    }

    public async Task RollbackTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _postgresContainer.StartAsync(),
            _redisContainer.StartAsync(),
            _rabbitMqContainer.StartAsync()
        );

        _connectionString = _postgresContainer.GetConnectionString();
        _redisConnectionString = _redisContainer.GetConnectionString();
        _rabbitMqConnectionString = _rabbitMqContainer.GetConnectionString();
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return DisposeAsync().AsTask();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
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
                ["CORS:AllowedOrigins:0"] = "http://localhost:5000",
                ["IAM:Url"] = "http://iamservice",
                ["Jwt:SecurityKey"] = "test-secret-key-for-integration-tests-minimum-32-chars"
            })
            .Build();
#pragma warning restore CS0618

        builder.UseConfiguration(config);

        builder.ConfigureTestServices(services =>
        {
            // Clear all DbContext registrations
            var dbContextDescriptors = services.Where(d => d.ServiceType.IsAssignableFrom(typeof(SupplierDbContext))).ToList();
            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }

            // Add test-specific DbContext
            services.AddDbContext<SupplierDbContext>(options =>
            {
                options.UseNpgsql(ConnectionString);
            });

            // Also register ISupplierDbContext
            services.AddScoped<ISupplierDbContext>(sp => sp.GetRequiredService<SupplierDbContext>());

            // Ensure database is created (once per factory, not per test)
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

            // Replace the real IAM HTTP client with a no-op stub so Polly retries
            // against the unreachable iamservice:80 host do not slow down tests.
            // PermissionAuthorizationHandler will fall back to JWT claims immediately.
            var iamClient = Substitute.For<IIamServiceClient>();
            iamClient.CheckPermissionAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(false));
            iamClient.GetUserPermissionsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<string>>([]));
            iamClient.CheckPermissionsAsync(Arg.Any<string>(), Arg.Any<IEnumerable<PermissionCheckRequest>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new Dictionary<string, bool>()));
            iamClient.GetAuthorizedResourcesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IEnumerable<string>>([]));
            services.AddScoped<IIamServiceClient>(_ => iamClient);
        });
    }

    private static readonly Dictionary<string, string[]> _testPermissions = new();

    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
        return client;
    }

    public HttpClient CreatePermissionAuthenticatedClient(string userId = "test-user", string[]? permissions = null)
    {
        var key = Guid.NewGuid().ToString();
        _testPermissions[key] = permissions ?? [];

        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme, key);
        return client;
    }

    public static string[] GetPermissions(string authKey)
    {
        if (string.IsNullOrEmpty(authKey) || !_testPermissions.TryGetValue(authKey, out var permissions))
        {
            // Default to all permissions if no key provided
            return
            [
                "supplier.suppliers.read",
                "supplier.suppliers.create",
                "supplier.suppliers.update",
                "supplier.suppliers.delete",
                "supplier.suppliers.approve",
                "supplier.contacts.read",
                "supplier.contacts.create",
                "supplier.certifications.manage",
                "supplier.performance.rate",
                "supplier.performance.view"
            ];
        }
        return permissions;
    }
}

[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>
{
}
