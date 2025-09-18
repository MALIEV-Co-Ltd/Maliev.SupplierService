using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Maliev.SupplierService.Api.Controllers;
using Maliev.SupplierService.Api.Models;
using Maliev.SupplierService.Data.DbContexts;
using Maliev.SupplierService.Data.Entities;

namespace Maliev.SupplierService.Tests.Controllers;

public class SuppliersControllerTests : IDisposable
{
    private readonly SupplierDbContext _context;
    private readonly SuppliersController _controller;
    private readonly Mock<ILogger<SuppliersController>> _mockLogger;

    public SuppliersControllerTests()
    {
        var options = new DbContextOptionsBuilder<SupplierDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SupplierDbContext(options);
        _mockLogger = new Mock<ILogger<SuppliersController>>();
        _controller = new SuppliersController(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task GetSuppliers_ReturnsEmptyList_WhenNoSuppliersExist()
    {
        // Act
        var result = await _controller.GetSuppliers();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var suppliers = okResult!.Value as IEnumerable<SupplierDto>;
        suppliers.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSuppliers_ReturnsSuppliers_WhenSuppliersExist()
    {
        // Arrange
        var category = new SupplierCategory { Name = "Test Category", IsActive = true };
        _context.SupplierCategories.Add(category);
        await _context.SaveChangesAsync();

        var supplier = new Supplier
        {
            Name = "Test Supplier",
            Status = SupplierStatus.Active,
            CategoryId = category.Id
        };
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetSuppliers();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var suppliers = okResult!.Value as IEnumerable<SupplierDto>;
        suppliers.Should().HaveCount(1);
        suppliers!.First().Name.Should().Be("Test Supplier");
    }

    [Fact]
    public async Task GetSupplier_ReturnsNotFound_WhenSupplierDoesNotExist()
    {
        // Act
        var result = await _controller.GetSupplier(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetSupplier_ReturnsSupplier_WhenSupplierExists()
    {
        // Arrange
        var supplier = new Supplier { Name = "Test Supplier", Status = SupplierStatus.Active };
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetSupplier(supplier.Id);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var supplierDto = okResult!.Value as SupplierDto;
        supplierDto!.Name.Should().Be("Test Supplier");
    }

    [Fact]
    public async Task CreateSupplier_ReturnsCreatedResult_WithValidRequest()
    {
        // Arrange
        var request = new CreateSupplierRequest
        {
            Name = "New Supplier",
            Status = SupplierStatus.Pending,
            Website = "https://example.com"
        };

        // Act
        var result = await _controller.CreateSupplier(request);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        var supplierDto = createdResult!.Value as SupplierDto;
        supplierDto!.Name.Should().Be("New Supplier");

        var supplierInDb = await _context.Suppliers.FirstOrDefaultAsync(s => s.Name == "New Supplier");
        supplierInDb.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateSupplier_ReturnsNotFound_WhenSupplierDoesNotExist()
    {
        // Arrange
        var request = new UpdateSupplierRequest
        {
            Name = "Updated Supplier",
            Status = SupplierStatus.Active
        };

        // Act
        var result = await _controller.UpdateSupplier(999, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateSupplier_ReturnsNoContent_WhenSupplierExists()
    {
        // Arrange
        var supplier = new Supplier { Name = "Original Supplier", Status = SupplierStatus.Pending };
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var request = new UpdateSupplierRequest
        {
            Name = "Updated Supplier",
            Status = SupplierStatus.Active
        };

        // Act
        var result = await _controller.UpdateSupplier(supplier.Id, request);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        var updatedSupplier = await _context.Suppliers.FindAsync(supplier.Id);
        updatedSupplier!.Name.Should().Be("Updated Supplier");
        updatedSupplier.Status.Should().Be(SupplierStatus.Active);
    }

    [Fact]
    public async Task DeleteSupplier_ReturnsNotFound_WhenSupplierDoesNotExist()
    {
        // Act
        var result = await _controller.DeleteSupplier(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteSupplier_ReturnsNoContent_WhenSupplierExists()
    {
        // Arrange
        var supplier = new Supplier { Name = "Test Supplier", Status = SupplierStatus.Active };
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteSupplier(supplier.Id);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        var deletedSupplier = await _context.Suppliers.FindAsync(supplier.Id);
        deletedSupplier.Should().BeNull();
    }

    [Fact]
    public async Task GetSuppliers_FiltersCorrectly_ByStatus()
    {
        // Arrange
        _context.Suppliers.AddRange(
            new Supplier { Name = "Active Supplier", Status = SupplierStatus.Active },
            new Supplier { Name = "Pending Supplier", Status = SupplierStatus.Pending }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetSuppliers(status: SupplierStatus.Active);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var suppliers = okResult!.Value as IEnumerable<SupplierDto>;
        suppliers.Should().HaveCount(1);
        suppliers!.First().Name.Should().Be("Active Supplier");
    }

    [Fact]
    public async Task GetSuppliers_SearchesCorrectly_ByName()
    {
        // Arrange
        _context.Suppliers.AddRange(
            new Supplier { Name = "ACME Corporation", Status = SupplierStatus.Active },
            new Supplier { Name = "Global Tech", Status = SupplierStatus.Active }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetSuppliers(search: "ACME");

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var suppliers = okResult!.Value as IEnumerable<SupplierDto>;
        suppliers.Should().HaveCount(1);
        suppliers!.First().Name.Should().Be("ACME Corporation");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}