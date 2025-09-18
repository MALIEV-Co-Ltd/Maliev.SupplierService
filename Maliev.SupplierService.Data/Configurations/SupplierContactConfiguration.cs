using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maliev.SupplierService.Data.Entities;

namespace Maliev.SupplierService.Data.Configurations;

public class SupplierContactConfiguration : IEntityTypeConfiguration<SupplierContact>
{
    public void Configure(EntityTypeBuilder<SupplierContact> builder)
    {
        builder.ToTable("SupplierContacts");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        // Configure properties
        builder.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.LastName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(254);
        builder.Property(c => c.Phone).HasMaxLength(20);
        builder.Property(c => c.Mobile).HasMaxLength(20);
        builder.Property(c => c.JobTitle).HasMaxLength(100);
        builder.Property(c => c.Department).HasMaxLength(100);
        builder.Property(c => c.Notes).HasMaxLength(500);

        builder.Property(c => c.Role)
               .HasConversion<int>()
               .HasDefaultValue(ContactRole.Primary);

        builder.Property(c => c.IsPrimary).HasDefaultValue(false);
        builder.Property(c => c.IsActive).HasDefaultValue(true);

        // Configure audit fields
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(c => c.UpdatedAt).HasDefaultValueSql("NOW()");

        // Configure indexes
        builder.HasIndex(c => c.SupplierId);
        builder.HasIndex(c => c.Email);
        builder.HasIndex(c => c.Role);
        builder.HasIndex(c => c.IsPrimary);
        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => c.CreatedAt);

        // Composite index for supplier + primary contact
        builder.HasIndex(c => new { c.SupplierId, c.IsPrimary });
    }
}