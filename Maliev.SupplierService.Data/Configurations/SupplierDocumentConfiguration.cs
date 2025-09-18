using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Maliev.SupplierService.Data.Entities;

namespace Maliev.SupplierService.Data.Configurations;

public class SupplierDocumentConfiguration : IEntityTypeConfiguration<SupplierDocument>
{
    public void Configure(EntityTypeBuilder<SupplierDocument> builder)
    {
        builder.ToTable("SupplierDocuments");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedOnAdd();

        // Configure properties
        builder.Property(d => d.Title).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Description).HasMaxLength(500);
        builder.Property(d => d.FileName).HasMaxLength(200);
        builder.Property(d => d.ContentType).HasMaxLength(100);
        builder.Property(d => d.UploadServiceFileId).HasMaxLength(100);
        builder.Property(d => d.Notes).HasMaxLength(500);

        builder.Property(d => d.Type)
               .HasConversion<int>()
               .HasDefaultValue(DocumentType.Contract);

        builder.Property(d => d.IsRequired).HasDefaultValue(false);
        builder.Property(d => d.IsActive).HasDefaultValue(true);

        // Configure audit fields
        builder.Property(d => d.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(d => d.UpdatedAt).HasDefaultValueSql("NOW()");

        // Configure indexes
        builder.HasIndex(d => d.SupplierId);
        builder.HasIndex(d => d.Type);
        builder.HasIndex(d => d.ValidFrom);
        builder.HasIndex(d => d.ValidTo);
        builder.HasIndex(d => d.IsRequired);
        builder.HasIndex(d => d.IsActive);
        builder.HasIndex(d => d.CreatedAt);
        builder.HasIndex(d => d.UploadServiceFileId);

        // Composite indexes
        builder.HasIndex(d => new { d.SupplierId, d.Type });
        builder.HasIndex(d => new { d.SupplierId, d.IsRequired });
    }
}