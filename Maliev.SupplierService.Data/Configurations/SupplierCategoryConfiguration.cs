using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maliev.SupplierService.Data.Entities;

namespace Maliev.SupplierService.Data.Configurations;

public class SupplierCategoryConfiguration : IEntityTypeConfiguration<SupplierCategory>
{
    public void Configure(EntityTypeBuilder<SupplierCategory> builder)
    {
        builder.ToTable("SupplierCategories");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        // Configure properties
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.IsActive).HasDefaultValue(true);

        // Configure audit fields
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(c => c.UpdatedAt).HasDefaultValueSql("NOW()");

        // Configure indexes
        builder.HasIndex(c => c.Name).IsUnique();
        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => c.CreatedAt);
    }
}