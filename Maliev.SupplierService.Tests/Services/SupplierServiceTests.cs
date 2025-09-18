using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Maliev.SupplierService.Api.Services;
using Maliev.SupplierService.Data.DbContexts;
using Maliev.SupplierService.Data.Entities;
using Maliev.SupplierService.Api.Models;

namespace Maliev.SupplierService.Tests.Services;

public class SupplierServiceTests : IDisposable
{
    private readonly SupplierDbContext _context;
    private readonly Mock<ILogger<Api.Services.SupplierService>> _loggerMock;
    private readonly Api.Services.SupplierService _supplierService;

    public SupplierServiceTests()
    {
        var options = new DbContextOptionsBuilder<SupplierDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SupplierDbContext(options);
        _loggerMock = new Mock<ILogger<Api.Services.SupplierService>>();
        _supplierService = new Api.Services.SupplierService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetSuppliersAsync_ShouldReturnSuppliers_WhenSuppliersExist()
    {
        // Arrange
        var category = new Data.Entities.SupplierCategory { Name = "Test Category", IsActive = true };
        var supplier = new Data.Entities.Supplier
        {
            Name = "Test Supplier",
            Status = SupplierStatus.Active,
            Category = category
        };

        _context.SupplierCategories.Add(category);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        // Act
        var result = await _supplierService.GetSuppliersAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Test Supplier");
    }

    [Fact]
    public async Task GetSupplierByIdAsync_ShouldReturnSupplier_WhenSupplierExists()
    {
        // Arrange
        var supplierId = 1;
        var category = new Data.Entities.SupplierCategory { Name = "Test Category", IsActive = true };
        var supplier = new Data.Entities.Supplier
        {
            Name = "Test Supplier",
            Status = SupplierStatus.Active,
            Category = category
        };

        _context.SupplierCategories.Add(category);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        // Act
        var result = await _supplierService.GetSupplierByIdAsync(supplierId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(supplierId);
        result.Name.Should().Be("Test Supplier");
    }

    [Fact]
    public async Task GetSupplierByIdAsync_ShouldReturnNull_WhenSupplierDoesNotExist()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var result = await _supplierService.GetSupplierByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateSupplierAsync_ShouldCreateSupplier_WithValidData()
    {
        // Arrange
        var categoryId = 1;
        var category = new Data.Entities.SupplierCategory { Id = categoryId, Name = "Test Category", IsActive = true };
        _context.SupplierCategories.Add(category);
        await _context.SaveChangesAsync();

        var supplierDto = new SupplierDto
        {
            Name = "New Supplier",
            Status = SupplierStatus.Pending,
            CategoryId = categoryId
        };

        // Act
        var result = await _supplierService.CreateSupplierAsync(supplierDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Supplier");

        var createdSupplier = await _context.Suppliers.FindAsync(result.Id);
        createdSupplier.Should().NotBeNull();
        createdSupplier!.Name.Should().Be("New Supplier");
    }

    [Fact]
    public async Task UpdateSupplierAsync_ShouldUpdateSupplier_WhenSupplierExists()
    {
        // Arrange
        var supplierId = 1;
        var category = new Data.Entities.SupplierCategory { Name = "Test Category", IsActive = true };
        var supplier = new Data.Entities.Supplier
        {
            Name = "Original Name",
            Status = SupplierStatus.Pending,
            CategoryId = category.Id
        };

        _context.SupplierCategories.Add(category);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var updateDto = new SupplierDto
        {
            Name = "Updated Name",
            Status = SupplierStatus.Active,
            CategoryId = category.Id
        };

        // Act
        var result = await _supplierService.UpdateSupplierAsync(supplierId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Name.Should().Be("Updated Name");
        result.Status.Should().Be(SupplierStatus.Active);
    }

    [Fact]
    public async Task DeleteSupplierAsync_ShouldReturnTrue_WhenSupplierExists()
    {
        // Arrange
        var supplierId = 1;
        var supplier = new Data.Entities.Supplier
        {
            Name = "To Delete",
            Status = SupplierStatus.Active
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        // Act
        var result = await _supplierService.DeleteSupplierAsync(supplierId);

        // Assert
        result.Should().BeTrue();

        var deletedSupplier = await _context.Suppliers.FindAsync(supplierId);
        deletedSupplier.Should().BeNull();
    }

    [Fact]
    public async Task DeleteSupplierAsync_ShouldReturnFalse_WhenSupplierDoesNotExist()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var result = await _supplierService.DeleteSupplierAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}