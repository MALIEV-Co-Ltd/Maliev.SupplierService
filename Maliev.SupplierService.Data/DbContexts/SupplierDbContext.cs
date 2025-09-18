using Microsoft.EntityFrameworkCore;
using Maliev.SupplierService.Data.Entities;
using Maliev.SupplierService.Data.Configurations;

namespace Maliev.SupplierService.Data.DbContexts;

public class SupplierDbContext : DbContext
{
    public SupplierDbContext(DbContextOptions<SupplierDbContext> options) : base(options)
    {
    }

    public DbSet<Supplier> Suppliers { get; set; } = null!;
    public DbSet<SupplierCategory> SupplierCategories { get; set; } = null!;
    public DbSet<SupplierContact> SupplierContacts { get; set; } = null!;
    public DbSet<SupplierAddress> SupplierAddresses { get; set; } = null!;
    public DbSet<SupplierDocument> SupplierDocuments { get; set; } = null!;
    public DbSet<SupplierRating> SupplierRatings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations
        modelBuilder.ApplyConfiguration(new SupplierConfiguration());
        modelBuilder.ApplyConfiguration(new SupplierCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new SupplierContactConfiguration());
        modelBuilder.ApplyConfiguration(new SupplierAddressConfiguration());
        modelBuilder.ApplyConfiguration(new SupplierDocumentConfiguration());
        modelBuilder.ApplyConfiguration(new SupplierRatingConfiguration());
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<IAuditable>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                    entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                    break;
            }
        }
    }
}