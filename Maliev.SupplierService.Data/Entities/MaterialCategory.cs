using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maliev.SupplierService.Data.Entities;

/// <summary>
/// Material category entity for categorizing supplier capabilities
/// </summary>
/// <remarks>
/// Table and column names use snake_case via explicit [Table] and [Column] attributes.
/// Indexes are configured in MaterialCategoryConfiguration.
/// </remarks>
[Table("material_categories")]
public class MaterialCategory : IAuditableEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public required string Name { get; set; }

    [MaxLength(500)]
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

    // Navigation property (many-to-many)
    public ICollection<Supplier> Suppliers { get; set; } = [];
}
