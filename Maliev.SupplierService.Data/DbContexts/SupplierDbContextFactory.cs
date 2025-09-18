using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Maliev.SupplierService.Data.DbContexts;

public class SupplierDbContextFactory : IDesignTimeDbContextFactory<SupplierDbContext>
{
    public SupplierDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SupplierDbContext>();

        // Use the connection string from environment variable if available
        // For design-time operations, requires environment variable to be set
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SupplierDbContext");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings__SupplierDbContext environment variable must be set for design-time operations. " +
                "Example: set \"ConnectionStrings__SupplierDbContext=Host=localhost;Database=supplier_app_db;Username=postgres;Password=your_password\"");
        }

        optionsBuilder.UseNpgsql(connectionString);

        return new SupplierDbContext(optionsBuilder.Options);
    }
}