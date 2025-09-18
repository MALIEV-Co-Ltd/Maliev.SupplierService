using System.ComponentModel.DataAnnotations;

namespace Maliev.SupplierService.Data.Entities;

public class SupplierCategory : IAuditable
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();
}