using Maliev.SupplierService.Data.Entities;
using Xunit;

namespace Maliev.SupplierService.Tests.Unit;

public class EntityTests
{
    [Fact]
    public void MaterialCategory_Properties_AreAccessible()
    {
        var category = new MaterialCategory
        {
            Id = Guid.NewGuid(),
            Name = "Name",
            Description = "Desc",
            IsActive = true
        };
        Assert.Equal("Name", category.Name);
        Assert.Equal("Desc", category.Description);
        Assert.True(category.IsActive);
    }

    [Fact]
    public void SupplierCapability_Properties_AreAccessible()
    {
        var capability = new SupplierCapability
        {
            Id = Guid.NewGuid(),
            SupplierId = Guid.NewGuid(),
            Name = "Name",
            Description = "Desc",
            IsActive = true
        };
        Assert.Equal("Name", capability.Name);
    }
}
