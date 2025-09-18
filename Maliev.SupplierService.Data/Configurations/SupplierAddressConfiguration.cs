using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maliev.SupplierService.Data.Entities;

namespace Maliev.SupplierService.Data.Configurations;

public class SupplierAddressConfiguration : IEntityTypeConfiguration<SupplierAddress>
{
    public void Configure(EntityTypeBuilder<SupplierAddress> builder)
    {
        builder.ToTable("SupplierAddresses");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        // Configure properties
        builder.Property(a => a.Building).HasMaxLength(100);
        builder.Property(a => a.AddressLine1).IsRequired().HasMaxLength(200);
        builder.Property(a => a.AddressLine2).HasMaxLength(200);
        builder.Property(a => a.City).IsRequired().HasMaxLength(100);
        builder.Property(a => a.State).HasMaxLength(100);
        builder.Property(a => a.PostalCode).HasMaxLength(20);
        builder.Property(a => a.Notes).HasMaxLength(500);

        builder.Property(a => a.Type)
               .HasConversion<int>()
               .HasDefaultValue(AddressType.Office);

        builder.Property(a => a.IsPrimary).HasDefaultValue(false);
        builder.Property(a => a.IsActive).HasDefaultValue(true);

        // Configure audit fields
        builder.Property(a => a.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(a => a.UpdatedAt).HasDefaultValueSql("NOW()");

        // Configure indexes
        builder.HasIndex(a => a.SupplierId);
        builder.HasIndex(a => a.Type);
        builder.HasIndex(a => a.CountryId);
        builder.HasIndex(a => a.IsPrimary);
        builder.HasIndex(a => a.IsActive);
        builder.HasIndex(a => a.CreatedAt);

        // Composite indexes
        builder.HasIndex(a => new { a.SupplierId, a.Type });
        builder.HasIndex(a => new { a.SupplierId, a.IsPrimary });
    }
}