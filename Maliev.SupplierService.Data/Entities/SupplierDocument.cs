using System.ComponentModel.DataAnnotations;

namespace Maliev.SupplierService.Data.Entities;

public class SupplierDocument : IAuditable
{
    public int Id { get; set; }

    public int SupplierId { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }

    public DocumentType Type { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? FileName { get; set; }

    [MaxLength(100)]
    public string? ContentType { get; set; }

    public long? FileSize { get; set; }

    // Integration with UploadService
    public string? UploadServiceFileId { get; set; }

    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }

    public bool IsRequired { get; set; } = false;

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation properties
    public virtual Supplier Supplier { get; set; } = null!;
}