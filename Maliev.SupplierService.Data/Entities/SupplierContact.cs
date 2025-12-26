using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maliev.SupplierService.Data.Entities;

/// <summary>
/// Supplier contact entity for managing supplier contact information
/// </summary>
/// <remarks>
/// Table and column names use snake_case via explicit [Table] and [Column] attributes.
/// Indexes are configured in SupplierContactConfiguration.
/// </remarks>
[Table("supplier_contacts")]
public class SupplierContact
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

    [MaxLength(100)]
    [Column("role")]
    public string? Role { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("email")]
    public required string Email { get; set; }

    [MaxLength(50)]
    [Column("phone")]
    public string? Phone { get; set; }

    [Required]
    [Column("is_primary")]
    public bool IsPrimary { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // Navigation property
    [System.Text.Json.Serialization.JsonIgnore]
    public Supplier Supplier { get; set; } = null!;
}
