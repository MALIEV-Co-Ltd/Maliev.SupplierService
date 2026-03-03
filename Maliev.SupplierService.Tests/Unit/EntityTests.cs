using Maliev.SupplierService.Domain.Entities;
using Maliev.SupplierService.Domain.Enums;
using Xunit;

namespace Maliev.SupplierService.Tests.Unit;

public class EntityTests
{
    [Fact]
    public void Supplier_InitializesCollections()
    {
        // Act
        var supplier = new Supplier();

        // Assert
        Assert.NotNull(supplier.Contacts);
        Assert.NotNull(supplier.MaterialCategories);
        Assert.NotNull(supplier.Capabilities);
        Assert.NotNull(supplier.OnboardingHistory);
        Assert.NotNull(supplier.Certifications);
        Assert.NotNull(supplier.Evaluations);
    }

    [Fact]
    public void MaterialCategory_InitializesSuppliers()
    {
        // Act
        var category = new MaterialCategory
        {
            Id = Guid.NewGuid(),
            Name = "Metals",
            Description = "Raw metal materials"
        };

        // Assert
        Assert.NotNull(category.Suppliers);
        Assert.Equal("Metals", category.Name);
        Assert.Equal("Raw metal materials", category.Description);
    }

    [Fact]
    public void SupplierCapability_InitializesCorrectly()
    {
        // Act
        var capability = new SupplierCapability
        {
            Id = Guid.NewGuid(),
            Name = "Precision Machining",
            Description = "High precision CNC machining"
        };

        // Assert
        Assert.Equal("Precision Machining", capability.Name);
        Assert.Equal("High precision CNC machining", capability.Description);
    }
}
