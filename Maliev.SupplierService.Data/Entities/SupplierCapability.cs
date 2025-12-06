using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maliev.SupplierService.Data.Entities;

/// <summary>
/// Supplier capability entity tracking specific supplier capabilities
/// </summary>
/// <remarks>
/// Table and column names use snake_case via explicit [Table] and [Column] attributes.
/// Indexes are configured in SupplierCapabilityConfiguration.
/// </remarks>
[Table("supplier_capabilities")]
public class SupplierCapability
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("supplier_id")]
    public Guid SupplierId { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("name")]
    public required string Name { get; set; }

    [MaxLength(1000)]
    [Column("description")]
    public string? Description { get; set; }

    [Required]
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // Navigation property
    public Supplier Supplier { get; set; } = null!;
}
