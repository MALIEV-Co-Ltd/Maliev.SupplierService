using System.Net;
using System.Net.Http.Json;
using Maliev.SupplierService.Api.DTOs.Responses;
using Maliev.SupplierService.Api.Services.ExternalServices;
using Maliev.SupplierService.Tests.Integration.Infrastructure;
using NSubstitute;
using Xunit;

namespace Maliev.SupplierService.Tests.Integration;

[Collection(nameof(IntegrationTestCollection))]
public class DependencyTests : BaseIntegrationTest
{
    public DependencyTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DeleteSupplier_WithInvoices_Returns400Conflict()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync();

        // Mock InvoiceService to return references
        var invoiceClient = Factory.Services.GetService(typeof(IInvoiceServiceClient)) as IInvoiceServiceClient;
        invoiceClient!.CheckReferencesAsync(supplier.Id, Arg.Any<CancellationToken>())
            .Returns(new DependencyCheckResult(true, "InvoiceService", 5, null, false));

        // Act
        var response = await Client.DeleteAsync($"/supplier/v1/suppliers/{supplier.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var error = System.Text.Json.JsonSerializer.Deserialize<DependencyErrorResponse>(content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(error);
        Assert.NotNull(error!.Dependencies);
        Assert.Contains("InvoiceService", error.Dependencies.Select(d => d.ServiceName));
    }

    [Fact]
    public async Task DeleteSupplier_WithPurchaseOrders_Returns400Conflict()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync();

        // Mock PurchaseOrderService to return references
        var poClient = Factory.Services.GetService(typeof(IPurchaseOrderServiceClient)) as IPurchaseOrderServiceClient;
        poClient!.CheckReferencesAsync(supplier.Id, Arg.Any<CancellationToken>())
            .Returns(new DependencyCheckResult(true, "PurchaseOrderService", 3, null, false));

        // Act
        var response = await Client.DeleteAsync($"/supplier/v1/suppliers/{supplier.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<DependencyErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains("PurchaseOrderService", error!.Dependencies.Select(d => d.ServiceName));
    }
}
