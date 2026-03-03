using Maliev.SupplierService.Domain.Validation;
using Maliev.SupplierService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Maliev.SupplierService.Tests.Infrastructure;

public class ModelIntegrityTests
{
    [Fact]
    public void Model_ShouldNotHavePendingChanges()
    {
        var options = new DbContextOptionsBuilder<SupplierDbContext>()
            .UseNpgsql("Host=localhost;Database=ModelCheck")
            .Options;
        using var context = new SupplierDbContext(options);
        var hasChanges = context.Database.HasPendingModelChanges();
        Assert.False(hasChanges, "Run 'dotnet ef migrations add <Name> --project Maliev.SupplierService.Infrastructure.Persistence --startup-project Maliev.SupplierService.Api'");
    }
}
