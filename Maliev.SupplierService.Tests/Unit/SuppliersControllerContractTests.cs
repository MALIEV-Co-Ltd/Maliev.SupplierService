using System.Reflection;
using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.SupplierService.Api.Controllers;
using Maliev.SupplierService.Api.DTOs.Responses;
using Maliev.SupplierService.Application.Authorization;
using Maliev.SupplierService.Application.Interfaces;
using Maliev.SupplierService.Domain.Entities;
using Maliev.SupplierService.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Maliev.SupplierService.Tests.Unit;

public class SuppliersControllerContractTests
{
    [Fact]
    public void ValidateSupplier_DeclaresVersionedRouteAndReadPermission()
    {
        // Arrange
        var controllerType = typeof(SuppliersController);
        var method = controllerType.GetMethod(nameof(SuppliersController.ValidateSupplier));

        // Act
        var apiVersion = controllerType.GetCustomAttribute<ApiVersionAttribute>();
        var controllerRoute = controllerType.GetCustomAttribute<RouteAttribute>();
        var httpGet = method?.GetCustomAttribute<HttpGetAttribute>();
        var permission = method?.GetCustomAttribute<RequirePermissionAttribute>();

        // Assert
        Assert.NotNull(method);
        Assert.NotNull(apiVersion);
        Assert.Contains(apiVersion.Versions, version => version.MajorVersion == 1);
        Assert.Equal("supplier/v{version:apiVersion}/suppliers", controllerRoute?.Template);
        Assert.Equal("{id:guid}/validate", httpGet?.Template);
        Assert.Equal(SupplierPermissions.Suppliers.Read, permission?.Permission);
    }

    [Fact]
    public async Task ValidateSupplier_ActiveSupplier_ReturnsConsumerProjection()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var supplier = new Supplier
        {
            Id = supplierId,
            CompanyName = "Contract Supplier",
            TaxId = "123456789",
            Status = SupplierStatus.Active
        };
        var supplierService = Substitute.For<ISupplierService>();
        supplierService.ValidateSupplierAsync(supplierId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<(bool IsValid, Supplier? Supplier)>((true, supplier)));
        var controller = new SuppliersController(
            supplierService,
            Substitute.For<ILogger<SuppliersController>>());

        // Act
        var response = await controller.ValidateSupplier(supplierId, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var projection = Assert.IsType<SupplierValidationResponse>(ok.Value);
        Assert.Equal(supplierId, projection.Id);
        Assert.Equal("Contract Supplier", projection.CompanyName);
        Assert.Equal("123456789", projection.TaxId);
        Assert.Equal(SupplierStatus.Active, projection.Status);
        Assert.True(projection.IsActive);
    }

    [Fact]
    public async Task ValidateSupplier_MissingSupplier_Returns404()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var supplierService = Substitute.For<ISupplierService>();
        supplierService.ValidateSupplierAsync(supplierId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<(bool IsValid, Supplier? Supplier)>((false, null)));
        var controller = new SuppliersController(
            supplierService,
            Substitute.For<ILogger<SuppliersController>>());

        // Act
        var response = await controller.ValidateSupplier(supplierId, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(response.Result);
    }

    [Fact]
    public async Task ValidateSupplier_PassesRequestCancellationTokenToApplicationService()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        using var cancellationSource = new CancellationTokenSource();
        var cancellationToken = cancellationSource.Token;
        var supplierService = Substitute.For<ISupplierService>();
        supplierService.ValidateSupplierAsync(supplierId, cancellationToken)
            .Returns(Task.FromResult<(bool IsValid, Supplier? Supplier)>((false, null)));
        var controller = new SuppliersController(
            supplierService,
            Substitute.For<ILogger<SuppliersController>>());

        // Act
        var response = await controller.ValidateSupplier(supplierId, cancellationToken);

        // Assert
        Assert.IsType<NotFoundObjectResult>(response.Result);
        await supplierService.Received(1).ValidateSupplierAsync(supplierId, cancellationToken);
    }

    [Fact]
    public void GetSupplierReference_DeclaresVersionedRouteAndReadPermission()
    {
        // Arrange
        var controllerType = typeof(SuppliersController);
        var method = controllerType.GetMethod(nameof(SuppliersController.GetSupplierReference));

        // Act
        var apiVersion = controllerType.GetCustomAttribute<ApiVersionAttribute>();
        var controllerRoute = controllerType.GetCustomAttribute<RouteAttribute>();
        var httpGet = method?.GetCustomAttribute<HttpGetAttribute>();
        var permission = method?.GetCustomAttribute<RequirePermissionAttribute>();

        // Assert
        Assert.NotNull(method);
        Assert.NotNull(apiVersion);
        Assert.Contains(apiVersion.Versions, version => version.MajorVersion == 1);
        Assert.Equal("supplier/v{version:apiVersion}/suppliers", controllerRoute?.Template);
        Assert.Equal("{id:guid}/reference", httpGet?.Template);
        Assert.Equal(SupplierPermissions.SupplierReferences.Read, permission?.Permission);
    }

    [Fact]
    public async Task GetSupplierReference_ActiveSupplier_ReturnsMinimalProjection()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var supplier = new Supplier
        {
            Id = supplierId,
            CompanyName = "Contract Supplier",
            TaxId = "123456789",
            Status = SupplierStatus.Active
        };
        var supplierService = Substitute.For<ISupplierService>();
        supplierService.ValidateSupplierAsync(supplierId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<(bool IsValid, Supplier? Supplier)>((true, supplier)));
        var controller = new SuppliersController(
            supplierService,
            Substitute.For<ILogger<SuppliersController>>());

        // Act
        var response = await controller.GetSupplierReference(supplierId, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var projection = Assert.IsType<SupplierReferenceResponse>(ok.Value);
        Assert.Equal(supplierId, projection.Id);
        Assert.Equal("Contract Supplier", projection.CompanyName);
        Assert.True(projection.IsActive);
    }

    [Fact]
    public async Task GetSupplierReference_MissingSupplier_Returns404()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var supplierService = Substitute.For<ISupplierService>();
        supplierService.ValidateSupplierAsync(supplierId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<(bool IsValid, Supplier? Supplier)>((false, null)));
        var controller = new SuppliersController(
            supplierService,
            Substitute.For<ILogger<SuppliersController>>());

        // Act
        var response = await controller.GetSupplierReference(supplierId, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(response.Result);
    }

    [Fact]
    public async Task GetSupplierReference_PassesRequestCancellationTokenToApplicationService()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        using var cancellationSource = new CancellationTokenSource();
        var cancellationToken = cancellationSource.Token;
        var supplierService = Substitute.For<ISupplierService>();
        supplierService.ValidateSupplierAsync(supplierId, cancellationToken)
            .Returns(Task.FromResult<(bool IsValid, Supplier? Supplier)>((false, null)));
        var controller = new SuppliersController(
            supplierService,
            Substitute.For<ILogger<SuppliersController>>());

        // Act
        var response = await controller.GetSupplierReference(supplierId, cancellationToken);

        // Assert
        Assert.IsType<NotFoundObjectResult>(response.Result);
        await supplierService.Received(1).ValidateSupplierAsync(supplierId, cancellationToken);
    }

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
