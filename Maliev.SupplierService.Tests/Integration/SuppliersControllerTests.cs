using System.Net;
using System.Net.Http.Json;
using Maliev.SupplierService.Api.DTOs.Requests;
using Maliev.SupplierService.Api.DTOs.Responses;
using Maliev.SupplierService.Application.DTOs.Requests;
using Maliev.SupplierService.Domain.Enums;
using Maliev.SupplierService.Tests.Integration.Infrastructure;
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
        var supplier = await CreateTestSupplierAsync();

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
        var supplier = await CreateTestSupplierAsync();
        var request = new UpdateSupplierRequest(
            CompanyName: "Updated Name",
            Address: "Updated Address",
            City: null,
            Country: null,
            PostalCode: null,
            MaterialCategoryIds: null,
            Capabilities: null,
            RowVersion: Convert.ToBase64String(supplier.RowVersion)
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/supplier/v1/suppliers/{supplier.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await GetResponseAsync<SupplierResponse>(response);
        Assert.Equal("Updated Name", updated!.CompanyName);
    }
}
