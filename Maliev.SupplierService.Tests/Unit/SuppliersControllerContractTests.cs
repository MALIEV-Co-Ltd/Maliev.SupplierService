using Maliev.SupplierService.Api.Controllers;
using Maliev.SupplierService.Application.Interfaces;
using Maliev.SupplierService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Maliev.SupplierService.Tests.Unit;

public class SuppliersControllerContractTests
{
    [Fact]
    public async Task GetSupplier_PassesRequestCancellationTokenToApplicationService()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        using var cancellationSource = new CancellationTokenSource();
        var cancellationToken = cancellationSource.Token;
        var supplierService = Substitute.For<ISupplierService>();
        supplierService.GetByIdAsync(supplierId, cancellationToken)
            .Returns(Task.FromResult<(Supplier? Supplier, uint Xmin)>((null, 0u)));
        var controller = new SuppliersController(
            supplierService,
            Substitute.For<ILogger<SuppliersController>>());

        // Act
        var response = await controller.GetSupplier(supplierId, cancellationToken);

        // Assert
        Assert.IsType<NotFoundObjectResult>(response.Result);
        await supplierService.Received(1).GetByIdAsync(supplierId, cancellationToken);
    }
}
