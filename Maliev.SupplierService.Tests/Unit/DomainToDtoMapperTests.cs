using Maliev.SupplierService.Api.DTOs.Responses;
using Maliev.SupplierService.Api.Mapping;
using Maliev.SupplierService.Domain.Entities;
using Maliev.SupplierService.Domain.Enums;
using Xunit;

namespace Maliev.SupplierService.Tests.Unit;

public class DomainToDtoMapperTests
{
    [Fact]
    public void ToSupplierResponse_MapsCorrectly()
    {
        // Arrange
        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            CompanyName = "Test",
            TaxId = "123",
            Status = SupplierStatus.Active,
            OnboardingStage = OnboardingStage.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var result = supplier.ToSupplierResponse(12345u);

        // Assert
        Assert.Equal(supplier.Id, result.Id);
        Assert.Equal(supplier.CompanyName, result.CompanyName);
        Assert.Equal("12345", result.RowVersion);
    }
}
