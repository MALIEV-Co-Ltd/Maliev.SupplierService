using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Maliev.SupplierService.Domain.Entities;
using Maliev.SupplierService.Domain.Enums;
using Maliev.SupplierService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.SupplierService.Tests.Integration.Infrastructure;

public abstract class BaseIntegrationTest : IAsyncLifetime
{
    protected readonly IntegrationTestWebAppFactory Factory;
    protected readonly HttpClient Client;
    protected readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        Factory = factory;
        Client = factory.CreateAuthenticatedClient();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }

    // Phase 3: Transaction support for test isolation
    protected async Task RunInTransactionAsync(Func<Task> testAction)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SupplierDbContext>();

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            await testAction();
        }
        finally
        {
            await transaction.RollbackAsync();
        }
    }

    protected async Task<T> RunInTransactionAsync<T>(Func<Task<T>> testAction)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SupplierDbContext>();

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            return await testAction();
        }
        finally
        {
            await transaction.RollbackAsync();
        }
    }

    protected async Task<T?> GetResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    protected async Task<(Supplier Supplier, uint Xmin)> CreateTestSupplierAsync(
        string name = "Test Supplier",
        string taxId = null!)
    {
        taxId ??= Guid.NewGuid().ToString().Substring(0, 8);
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SupplierDbContext>();

        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            CompanyName = name,
            TaxId = taxId,
            Address = "123 Test St",
            City = "Test City",
            Country = "Test Country",
            Status = SupplierStatus.PendingApproval,
            OnboardingStage = OnboardingStage.PendingApproval,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Suppliers.Add(supplier);
        await context.SaveChangesAsync();

        // Reload to get the xmin value from the database
        await context.Entry(supplier).ReloadAsync();

        // Get the xmin value from the shadow property
        var xminValue = context.Entry(supplier).Property<uint>("xmin").CurrentValue;

        return (supplier, xminValue);
    }

    protected async Task<MaterialCategory> CreateTestCategoryAsync(string name = "Test Category")
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SupplierDbContext>();

        var category = new MaterialCategory
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "Test Description",
            IsActive = true
        };

        context.MaterialCategories.Add(category);
        await context.SaveChangesAsync();

        return category;
    }
}
