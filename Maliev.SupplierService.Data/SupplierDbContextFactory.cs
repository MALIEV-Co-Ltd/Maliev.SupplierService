using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Maliev.SupplierService.Data;

/// <summary>
/// Design-time factory for SupplierDbContext
/// </summary>
/// <remarks>
/// Used by EF Core tools (dotnet ef migrations add, dotnet ef database update)
/// to create DbContext instances at design time.
/// Reads connection string from environment variable or uses default localhost connection.
/// </remarks>
public class SupplierDbContextFactory : IDesignTimeDbContextFactory<SupplierDbContext>
{
    /// <summary>
    /// Creates a new instance of a design-time context.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>A new <see cref="SupplierDbContext"/> instance.</returns>
    public SupplierDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SupplierDbContext>();

        // Use hardcoded connection string for design-time operations
        var connectionString = "Host=localhost;Database=supplier_design;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new SupplierDbContext(optionsBuilder.Options);
    }
}
