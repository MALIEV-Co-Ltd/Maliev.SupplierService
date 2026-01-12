using System.Net;
using System.Net.Http.Json;
using Maliev.SupplierService.Api.DTOs.Requests;
using Maliev.SupplierService.Api.DTOs.Responses;
using Maliev.SupplierService.Data.Enums;
using Maliev.SupplierService.Tests.Integration.Infrastructure;

namespace Maliev.SupplierService.Tests.Integration;

[Collection(nameof(IntegrationTestCollection))]
public class SuppliersControllerTests : BaseIntegrationTest
{
    public SuppliersControllerTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateSupplier_WithValidData_Returns201AndSupplier()
    {
        // Arrange
        var request = new CreateSupplierRequest(
            CompanyName: "Acme Corporation",
            TaxId: "ACME123456",
            Address: "123 Business Ave",
            City: "New York",
            Country: "USA",
            PostalCode: "10001",
            MaterialCategoryIds: null,
            Capabilities: ["Manufacturing", "Assembly"],
            PrimaryContact: new CreateContactRequest(
                Name: "John Doe",
                Email: "john@acme.com",
                Role: "Procurement Manager",
                Phone: "+1234567890"
            )
        );

        // Act
        var response = await Client.PostAsJsonAsync("/supplier/v1/suppliers", request);

        // Debug
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Expected 201, got {response.StatusCode}: {content}");
        }

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var supplier = await GetResponseAsync<SupplierResponse>(response);
        Assert.NotNull(supplier);
        Assert.Equal("Acme Corporation", supplier!.CompanyName);
        Assert.Equal("ACME123456", supplier.TaxId);
        Assert.Equal(SupplierStatus.PendingApproval, supplier.Status);
        Assert.NotEqual(Guid.Empty, supplier.Id);
    }

    [Fact]
    public async Task CreateSupplier_WithDuplicateTaxId_Returns400BadRequest()
    {
        // Arrange
        var existingSupplier = await CreateTestSupplierAsync(taxId: "DUPLICATE123");

        var request = new CreateSupplierRequest(
            CompanyName: "Another Company",
            TaxId: "DUPLICATE123",
            Address: "456 Other Street",
            City: "Chicago",
            Country: "USA",
            PostalCode: "60601",
            MaterialCategoryIds: null,
            Capabilities: null,
            PrimaryContact: null
        );

        // Act
        var response = await Client.PostAsJsonAsync("/supplier/v1/suppliers", request);

        // Assert - InvalidOperationException maps to BadRequest in the middleware
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateSupplier_WithInvalidData_Returns400BadRequest()
    {
        // Arrange
        var request = new CreateSupplierRequest(
            CompanyName: "", // Required field empty
            TaxId: "",
            Address: "",
            City: "",
            Country: "",
            PostalCode: null,
            MaterialCategoryIds: null,
            Capabilities: null,
            PrimaryContact: null
        );

        // Act
        var response = await Client.PostAsJsonAsync("/supplier/v1/suppliers", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetSupplier_ExistingId_Returns200WithDetails()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync(companyName: "Get Test Company");

        // Act
        var response = await Client.GetAsync($"/supplier/v1/suppliers/{supplier.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await GetResponseAsync<SupplierDetailResponse>(response);
        Assert.NotNull(result);
        Assert.Equal(supplier.Id, result!.Id);
        Assert.Equal("Get Test Company", result.CompanyName);
    }

    [Fact]
    public async Task GetSupplier_NonExistingId_Returns404NotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/supplier/v1/suppliers/{nonExistingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ListSuppliers_WithPagination_ReturnsPagedResults()
    {
        // Arrange
        await CreateTestSupplierAsync("Company A", "TAX001");
        await CreateTestSupplierAsync("Company B", "TAX002");
        await CreateTestSupplierAsync("Company C", "TAX003");

        // Act
        var response = await Client.GetAsync("/supplier/v1/suppliers?page=1&pageSize=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await GetResponseAsync<SupplierListResponse>(response);
        Assert.NotNull(result);
        Assert.Equal(2, result!.Items.Count);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task ListSuppliers_WithStatusFilter_ReturnsFilteredResults()
    {
        // Arrange
        await CreateTestSupplierAsync("Active Company", "TAX001", SupplierStatus.Active);
        await CreateTestSupplierAsync("Pending Company", "TAX002", SupplierStatus.PendingApproval);

        // Act
        var response = await Client.GetAsync("/supplier/v1/suppliers?status=Active");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await GetResponseAsync<SupplierListResponse>(response);
        Assert.NotNull(result);
        Assert.Single(result!.Items);
        Assert.Equal("Active Company", result.Items[0].CompanyName);
    }

    [Fact]
    public async Task DeleteSupplier_ExistingId_Returns204NoContent()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync();

        // Act
        var response = await Client.DeleteAsync($"/supplier/v1/suppliers/{supplier.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify deleted
        var getResponse = await Client.GetAsync($"/supplier/v1/suppliers/{supplier.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateSupplier_WithValidData_Returns200()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync();

        // Get current rowVersion
        var getResponse = await Client.GetAsync($"/supplier/v1/suppliers/{supplier.Id}");
        var current = await GetResponseAsync<SupplierDetailResponse>(getResponse);

        var request = new UpdateSupplierRequest(
            CompanyName: "Updated Company Name",
            Address: null,
            City: null,
            Country: null,
            PostalCode: null,
            MaterialCategoryIds: null,
            Capabilities: null,
            RowVersion: current!.RowVersion
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/supplier/v1/suppliers/{supplier.Id}", request);

        // Debug
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"BadRequest Details: {content}");
        }

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await GetResponseAsync<SupplierResponse>(response);
        Assert.Equal("Updated Company Name", result!.CompanyName);
    }

    [Fact]
    public async Task UpdateStatus_ToActive_Returns200()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync(status: SupplierStatus.PendingApproval);

        // Get current rowVersion
        var getResponse = await Client.GetAsync($"/supplier/v1/suppliers/{supplier.Id}");
        var current = await GetResponseAsync<SupplierDetailResponse>(getResponse);

        var request = new UpdateStatusRequest(
            Status: SupplierStatus.Active,
            Reason: "Approved by admin",
            RowVersion: current!.RowVersion
        );

        // Act
        var response = await Client.PatchAsJsonAsync($"/supplier/v1/suppliers/{supplier.Id}/status", request);

        // Debug
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"UpdateStatus BadRequest Details: {content}");
        }

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await GetResponseAsync<SupplierResponse>(response);
        Assert.Equal(SupplierStatus.Active, result!.Status);
    }
}
