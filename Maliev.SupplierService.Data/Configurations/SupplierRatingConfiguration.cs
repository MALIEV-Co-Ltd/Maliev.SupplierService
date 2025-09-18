using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maliev.SupplierService.Data.Entities;

namespace Maliev.SupplierService.Data.Configurations;

public class SupplierRatingConfiguration : IEntityTypeConfiguration<SupplierRating>
{
    public void Configure(EntityTypeBuilder<SupplierRating> builder)
    {
        builder.ToTable("SupplierRatings");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();

        // Configure properties
        builder.Property(r => r.RatingPeriod).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Comments).HasMaxLength(1000);
        builder.Property(r => r.ReviewedBy).HasMaxLength(100);

        // Configure decimal rating properties
        builder.Property(r => r.QualityRating).HasColumnType("decimal(3,2)");
        builder.Property(r => r.DeliveryRating).HasColumnType("decimal(3,2)");
        builder.Property(r => r.ServiceRating).HasColumnType("decimal(3,2)");
        builder.Property(r => r.PricingRating).HasColumnType("decimal(3,2)");
        builder.Property(r => r.CommunicationRating).HasColumnType("decimal(3,2)");
        builder.Property(r => r.OverallRating).HasColumnType("decimal(3,2)");

        // Configure audit fields
        builder.Property(r => r.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(r => r.UpdatedAt).HasDefaultValueSql("NOW()");

        // Configure indexes
        builder.HasIndex(r => r.SupplierId);
        builder.HasIndex(r => r.RatingDate);
        builder.HasIndex(r => r.RatingPeriod);
        builder.HasIndex(r => r.OverallRating);
        builder.HasIndex(r => r.CreatedAt);

        // Composite indexes
        builder.HasIndex(r => new { r.SupplierId, r.RatingDate });
        builder.HasIndex(r => new { r.SupplierId, r.RatingPeriod });
    }
}