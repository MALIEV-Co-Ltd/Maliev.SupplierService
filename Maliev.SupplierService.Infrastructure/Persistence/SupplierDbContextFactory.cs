using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Maliev.SupplierService.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for creating SupplierDbContext for EF Core migrations
/// </summary>
public class SupplierDbContextFactory : IDesignTimeDbContextFactory<SupplierDbContext>
{
    /// <summary>
    /// Creates a new instance of <see cref="SupplierDbContext"/> for design-time use.
    /// </summary>
    /// <param name="args">Arguments passed by the design-time tool.</param>
    /// <returns>A new instance of the database context.</returns>
    public SupplierDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SupplierDbContext>();

        // Use a dummy connection string for design-time operations
        // The actual connection string will be configured in Program.cs
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=supplier;Username=postgres;Password=postgres"
        );

        return new SupplierDbContext(optionsBuilder.Options);
    }
}
