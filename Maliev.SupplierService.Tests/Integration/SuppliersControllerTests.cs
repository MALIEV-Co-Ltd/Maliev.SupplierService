using System.Net;
using System.Net.Http.Json;
using Maliev.SupplierService.Api.DTOs.Requests;
using Maliev.SupplierService.Api.DTOs.Responses;
using Maliev.SupplierService.Application.DTOs.Requests;
using Maliev.SupplierService.Domain.Enums;
using Maliev.SupplierService.Infrastructure.Persistence;
using Maliev.SupplierService.Tests.Integration.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.SupplierService.Tests.Integration;

[Collection(nameof(IntegrationTestCollection))]
public class SuppliersControllerTests : BaseIntegrationTest
{
    public SuppliersControllerTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateSupplier_WithValidData_Returns201()
    {
        // Arrange
        var request = new CreateSupplierRequest(
            CompanyName: "Test Supplier",
            TaxId: "123456789",
            Address: "123 Main St",
            City: "Test City",
            Country: "Test Country",
            PostalCode: "12345",
            MaterialCategoryIds: null,
            Capabilities: ["Logistics", "Manufacturing"],
            PrimaryContact: new CreateContactRequest("John Doe", "john@example.com", "CEO", "+1234567890")
        );

        // Act
        var response = await Client.PostAsJsonAsync("/supplier/v1/suppliers", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var supplier = await GetResponseAsync<SupplierResponse>(response);
        Assert.NotNull(supplier);
        Assert.Equal("Test Supplier", supplier!.CompanyName);
        Assert.False(string.IsNullOrWhiteSpace(supplier.RowVersion));
        Assert.NotEqual("0", supplier.RowVersion);
    }

    [Fact]
    public async Task CreateGetUpdateSupplier_UsesFreshRowVersion_UpdatesSupplier()
    {
        // Arrange
        var taxId = Guid.NewGuid().ToString("N")[..12];
        var createRequest = new CreateSupplierRequest(
            CompanyName: "Row Version Supplier",
            TaxId: taxId,
            Address: "123 Main St",
            City: "Bangkok",
            Country: "Thailand",
            PostalCode: "10310",
            MaterialCategoryIds: null,
            Capabilities: ["CNC"],
            PrimaryContact: new CreateContactRequest("Niran Supplier", "niran@example.com", "Primary", "+6625550101")
        );

        var createResponse = await Client.PostAsJsonAsync("/supplier/v1/suppliers", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await GetResponseAsync<SupplierResponse>(createResponse);
        Assert.NotNull(created);

        var detailResponse = await Client.GetAsync($"/supplier/v1/suppliers/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        var detail = await GetResponseAsync<SupplierDetailResponse>(detailResponse);
        Assert.NotNull(detail);
        Assert.False(string.IsNullOrWhiteSpace(detail!.RowVersion));
        Assert.NotEqual("0", detail.RowVersion);
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SupplierDbContext>();
            var dbSupplier = await context.Suppliers.FirstAsync(s => s.Id == created.Id);
            await context.Entry(dbSupplier).ReloadAsync();
            var dbRowVersion = context.Entry(dbSupplier).Property<uint>("xmin").CurrentValue;
            Assert.Equal(dbRowVersion.ToString(), detail.RowVersion);
        }

        var updateRequest = new UpdateSupplierRequest(
            CompanyName: "Row Version Supplier Updated",
            Address: "99 Revised Road",
            City: "Samut Prakan",
            Country: "Thailand",
            PostalCode: "10270",
            MaterialCategoryIds: null,
            Capabilities: ["CNC", "Anodizing"],
            RowVersion: detail.RowVersion
        );

        // Act
        var updateResponse = await Client.PutAsJsonAsync($"/supplier/v1/suppliers/{created.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await GetResponseAsync<SupplierResponse>(updateResponse);
        Assert.NotNull(updated);
        Assert.Equal("Row Version Supplier Updated", updated!.CompanyName);
        Assert.False(string.IsNullOrWhiteSpace(updated.RowVersion));
        Assert.NotEqual("0", updated.RowVersion);
    }

    [Fact]
    public async Task ListSuppliers_ReturnsExpectedItems()
    {
        // Arrange
        await CreateTestSupplierAsync();
        await CreateTestSupplierAsync();

        // Act
        var response = await Client.GetAsync("/supplier/v1/suppliers?pageSize=10&page=1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await GetResponseAsync<SupplierListResponse>(response);
        Assert.NotNull(result);
        Assert.True(result!.TotalCount >= 2);
    }

    [Fact]
    public async Task GetSupplier_ExistingId_Returns200()
    {
        // Arrange
        var (supplier, _) = await CreateTestSupplierAsync();

        // Act
        var response = await Client.GetAsync($"/supplier/v1/suppliers/{supplier.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var detail = await GetResponseAsync<SupplierDetailResponse>(response);
        Assert.NotNull(detail);
        Assert.Equal(supplier.Id, detail!.Id);
    }

    [Fact]
    public async Task UpdateSupplier_ValidData_Returns200()
    {
        // Arrange
        var (supplier, xmin) = await CreateTestSupplierAsync();
        var request = new UpdateSupplierRequest(
            CompanyName: "Updated Name",
            Address: "Updated Address",
            City: null,
            Country: null,
            PostalCode: null,
            MaterialCategoryIds: null,
            Capabilities: null,
            RowVersion: xmin.ToString()
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/supplier/v1/suppliers/{supplier.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await GetResponseAsync<SupplierResponse>(response);
        Assert.Equal("Updated Name", updated!.CompanyName);
    }
}
