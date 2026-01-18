using Maliev.SupplierService.Api.Mapping;
using Maliev.SupplierService.Data.Entities;
using Maliev.SupplierService.Data.Enums;
using Xunit;

namespace Maliev.SupplierService.Tests.Unit;

public class DomainToDtoMapperTests
{
    [Fact]
    public void ToSupplierResponse_MapsCorrectlty()
    {
        // Arrange
        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            CompanyName = "Test Company",
            TaxId = "TAX123",
            Address = "123 Street",
            City = "City",
            Country = "Country",
            Status = SupplierStatus.Active,
            RowVersion = new byte[] { 1, 2, 3, 4 }
        };

        // Act
        var response = supplier.ToSupplierResponse();

        // Assert
        Assert.Equal(supplier.Id, response.Id);
        Assert.Equal(supplier.CompanyName, response.CompanyName);
        Assert.Equal(supplier.TaxId, response.TaxId);
        Assert.Equal(Convert.ToBase64String(supplier.RowVersion), response.RowVersion);
    }

    [Fact]
    public void ToSupplierDetailResponse_MapsCorrectlyWithCollections()
    {
        // Arrange
        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            CompanyName = "Test Company",
            TaxId = "TAX123",
            Address = "123 Street",
            City = "City",
            Country = "Country",
            RowVersion = new byte[] { 1, 2, 3, 4 },
            Contacts = new List<SupplierContact>
            {
                new() { Id = Guid.NewGuid(), Name = "Contact 1", Email = "test@example.com", IsPrimary = true }
            }
        };

        // Act
        var response = supplier.ToSupplierDetailResponse();

        // Assert
        Assert.Equal(supplier.Id, response.Id);
        Assert.Single(response.Contacts);
        Assert.Equal("Contact 1", response.Contacts[0].Name);
    }
}
