using System.Net;
using System.Net.Http.Json;
using Maliev.SupplierService.Api.DTOs.Responses;
using Maliev.SupplierService.Application.Interfaces;
using Maliev.SupplierService.Tests.Integration.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
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
        var (supplier, _) = await CreateTestSupplierAsync();

        // Mock InvoiceService to return references
        var invoiceClient = Factory.Services.GetRequiredService<IInvoiceServiceClient>();
        invoiceClient.CheckReferencesAsync(supplier.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((true, false)));

        // Act
        var response = await Client.DeleteAsync($"/supplier/v1/suppliers/{supplier.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("referenced by: InvoiceService", content);
    }

    [Fact]
    public async Task DeleteSupplier_WithPurchaseOrders_Returns400Conflict()
    {
        // Arrange
        var (supplier, _) = await CreateTestSupplierAsync();

        // Mock PurchaseOrderService to return references
        var poClient = Factory.Services.GetRequiredService<IPurchaseOrderServiceClient>();
        poClient.CheckReferencesAsync(supplier.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((true, false)));

        // Act
        var response = await Client.DeleteAsync($"/supplier/v1/suppliers/{supplier.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("referenced by: PurchaseOrderService", content);
    }
}
