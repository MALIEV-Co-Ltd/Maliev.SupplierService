using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Maliev.SupplierService.Data.Enums;

namespace Maliev.SupplierService.Data.Entities;

/// <summary>
/// Supplier certification entity for tracking compliance documents and certifications
/// </summary>
/// <remarks>
/// Table and column names use snake_case via explicit [Table] and [Column] attributes.
/// Indexes are configured in SupplierCertificationConfiguration.
/// </remarks>
[Table("supplier_certifications")]
public class SupplierCertification
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("supplier_id")]
    public Guid SupplierId { get; set; }

    [Required]
    [Column("document_type")]
    public CertificationType DocumentType { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("document_name")]
    public required string DocumentName { get; set; }

    [Required]
    [Column("issue_date")]
    public DateOnly IssueDate { get; set; }

    [Column("expiration_date")]
    public DateOnly? ExpirationDate { get; set; }

    [MaxLength(500)]
    [Column("external_file_ref")]
    public string? ExternalFileRef { get; set; }

    [MaxLength(1000)]
    [Column("notes")]
    public string? Notes { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // Navigation property
    public Supplier Supplier { get; set; } = null!;
}
