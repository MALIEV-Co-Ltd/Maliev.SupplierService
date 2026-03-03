using System.Net.Http.Json;
using System.Text.Json;
using Maliev.SupplierService.Domain.Entities;
using Maliev.SupplierService.Domain.Enums;
using Maliev.SupplierService.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.SupplierService.Tests.Integration.Infrastructure;

public abstract class BaseIntegrationTest : IAsyncLifetime
{
    protected readonly IntegrationTestWebAppFactory Factory;
    protected readonly HttpClient Client;
    protected readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        Factory = factory;
        Client = factory.CreateAuthenticatedClient();
    }

    public async Task InitializeAsync()
    {
        await Factory.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
    }

    protected async Task<T?> GetResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    protected async Task<Supplier> CreateTestSupplierAsync(
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

        return supplier;
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
