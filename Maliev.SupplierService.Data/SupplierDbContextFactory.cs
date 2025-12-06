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

        // Try to get connection string from environment variable first
        // This allows flexibility during migrations: dotnet ef migrations add --connection "..."
        var connectionString = Environment.GetEnvironmentVariable("SupplierDbContext")
            ?? throw new InvalidOperationException("Connection string 'SupplierDbContext' not found in environment variables. Please set the 'SupplierDbContext' environment variable.");

        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(SupplierDbContext).Assembly.GetName().Name);
            npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });

        // Enable sensitive data logging for development only
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }

        return new SupplierDbContext(optionsBuilder.Options);
    }
}
