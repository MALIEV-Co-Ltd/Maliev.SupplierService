using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maliev.SupplierService.Data.Entities;

namespace Maliev.SupplierService.Data.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedOnAdd();

        // Configure properties
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.RegistrationNumber).HasMaxLength(100);
        builder.Property(s => s.TaxId).HasMaxLength(50);
        builder.Property(s => s.Website).HasMaxLength(500);
        builder.Property(s => s.Description).HasMaxLength(1000);

        builder.Property(s => s.Status)
               .HasConversion<int>()
               .HasDefaultValue(SupplierStatus.Pending);

        builder.Property(s => s.PaymentTerms).HasMaxLength(50);
        builder.Property(s => s.MinimumOrderAmount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.CreditLimit).HasColumnType("decimal(18,2)");

        builder.Property(s => s.QualityRating).HasColumnType("decimal(3,2)");
        builder.Property(s => s.DeliveryRating).HasColumnType("decimal(3,2)");
        builder.Property(s => s.ServiceRating).HasColumnType("decimal(3,2)");

        // Configure audit fields
        builder.Property(s => s.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(s => s.UpdatedAt).HasDefaultValueSql("NOW()");

        // Configure relationships
        builder.HasOne(s => s.Category)
               .WithMany(c => c.Suppliers)
               .HasForeignKey(s => s.CategoryId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.Contacts)
               .WithOne(c => c.Supplier)
               .HasForeignKey(c => c.SupplierId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Addresses)
               .WithOne(a => a.Supplier)
               .HasForeignKey(a => a.SupplierId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Documents)
               .WithOne(d => d.Supplier)
               .HasForeignKey(d => d.SupplierId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Ratings)
               .WithOne(r => r.Supplier)
               .HasForeignKey(r => r.SupplierId)
               .OnDelete(DeleteBehavior.Cascade);

        // Configure indexes
        builder.HasIndex(s => s.Name);
        builder.HasIndex(s => s.RegistrationNumber);
        builder.HasIndex(s => s.TaxId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.CategoryId);
        builder.HasIndex(s => s.CreatedAt);
        builder.HasIndex(s => s.UpdatedAt);
    }
}