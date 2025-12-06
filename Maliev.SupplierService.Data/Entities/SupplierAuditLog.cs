using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maliev.SupplierService.Data.Entities;

/// <summary>
/// Audit log entity for tracking all changes to supplier-related data
/// </summary>
/// <remarks>
/// Table and column names use snake_case via explicit [Table] and [Column] attributes.
/// Indexes are configured in SupplierAuditLogConfiguration.
/// OldValues and NewValues are stored as JSONB.
/// </remarks>
[Table("supplier_audit_logs")]
public class SupplierAuditLog
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("supplier_id")]
    public Guid SupplierId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("change_type")]
    public required string ChangeType { get; set; } // CREATE, UPDATE, DELETE

    [Required]
    [MaxLength(100)]
    [Column("changed_by")]
    public required string ChangedBy { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("changed_by_name")]
    public required string ChangedByName { get; set; }

    [Required]
    [Column("timestamp")]
    public DateTime Timestamp { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("entity_type")]
    public required string EntityType { get; set; }

    [Required]
    [Column("entity_id")]
    public Guid EntityId { get; set; }

    [Column("old_values")]
    public string? OldValues { get; set; } // JSON

    [Column("new_values")]
    public string? NewValues { get; set; } // JSON

    // Navigation property
    public Supplier Supplier { get; set; } = null!;
}
