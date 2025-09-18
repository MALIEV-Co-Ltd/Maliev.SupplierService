using FluentAssertions;
using Maliev.SupplierService.Data.Entities;

namespace Maliev.SupplierService.Tests;

public class SimpleTests
{
    [Fact]
    public void Supplier_Constructor_SetsDefaultStatus()
    {
        // Arrange & Act
        var supplier = new Supplier
        {
            Name = "Test Supplier"
        };

        // Assert
        supplier.Name.Should().Be("Test Supplier");
        supplier.Status.Should().Be(SupplierStatus.Pending);
    }

    [Fact]
    public void SupplierCategory_Constructor_SetsDefaultActive()
    {
        // Arrange & Act
        var category = new SupplierCategory
        {
            Name = "Test Category"
        };

        // Assert
        category.Name.Should().Be("Test Category");
        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ContactRole_HasExpectedValues()
    {
        // Assert
        ContactRole.Primary.Should().Be((ContactRole)1);
        ContactRole.Procurement.Should().Be((ContactRole)2);
        ContactRole.Technical.Should().Be((ContactRole)3);
        ContactRole.Finance.Should().Be((ContactRole)4);
    }

    [Fact]
    public void SupplierStatus_HasExpectedValues()
    {
        // Assert
        SupplierStatus.Active.Should().Be((SupplierStatus)1);
        SupplierStatus.Inactive.Should().Be((SupplierStatus)2);
        SupplierStatus.Pending.Should().Be((SupplierStatus)3);
        SupplierStatus.Blocked.Should().Be((SupplierStatus)4);
    }
}