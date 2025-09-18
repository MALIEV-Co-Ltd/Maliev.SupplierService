using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Maliev.SupplierService.Api.Services;
using Maliev.SupplierService.Data.DbContexts;
using Maliev.SupplierService.Data.Entities;
using Maliev.SupplierService.Api.Models;

namespace Maliev.SupplierService.Tests.Services;

public class SupplierCategoryServiceTests : IDisposable
{
    private readonly SupplierDbContext _context;
    private readonly Mock<ILogger<SupplierCategoryService>> _loggerMock;
    private readonly SupplierCategoryService _categoryService;

    public SupplierCategoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<SupplierDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SupplierDbContext(options);
        _loggerMock = new Mock<ILogger<SupplierCategoryService>>();
        _categoryService = new SupplierCategoryService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetCategoriesAsync_ShouldReturnActiveCategories_WhenCategoriesExist()
    {
        // Arrange
        var activeCategory = new Data.Entities.SupplierCategory
        {
            Name = "Active Category",
            IsActive = true
        };

        var inactiveCategory = new Data.Entities.SupplierCategory
        {
            Name = "Inactive Category",
            IsActive = false
        };

        _context.SupplierCategories.AddRange(activeCategory, inactiveCategory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _categoryService.GetCategoriesAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Active Category");
        result.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldCreateCategory_WithValidData()
    {
        // Arrange
        var categoryDto = new SupplierCategoryDto
        {
            Name = "New Category",
            Description = "Test Description",
            IsActive = true
        };

        // Act
        var result = await _categoryService.CreateCategoryAsync(categoryDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Category");
        result.Description.Should().Be("Test Description");
        result.IsActive.Should().BeTrue();

        var createdCategory = await _context.SupplierCategories.FindAsync(result.Id);
        createdCategory.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldMarkAsInactive_WhenCategoryExists()
    {
        // Arrange
        var categoryId = 1;
        var category = new Data.Entities.SupplierCategory
        {
            Name = "To Delete",
            IsActive = true
        };

        _context.SupplierCategories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _categoryService.DeleteCategoryAsync(categoryId);

        // Assert
        result.Should().BeTrue();

        var updatedCategory = await _context.SupplierCategories.FindAsync(categoryId);
        updatedCategory.Should().NotBeNull();
        updatedCategory!.IsActive.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}