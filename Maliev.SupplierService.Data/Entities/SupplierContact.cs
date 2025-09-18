using System.ComponentModel.DataAnnotations;

namespace Maliev.SupplierService.Data.Entities;

public class SupplierContact : IAuditable
{
    public int Id { get; set; }

    public int SupplierId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string FirstName { get; set; }

    [Required]
    [MaxLength(100)]
    public required string LastName { get; set; }

    [Required]
    [MaxLength(254)]
    public required string Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(20)]
    public string? Mobile { get; set; }

    [MaxLength(100)]
    public string? JobTitle { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    public ContactRole Role { get; set; } = ContactRole.Primary;

    public bool IsPrimary { get; set; } = false;

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation properties
    public virtual Supplier Supplier { get; set; } = null!;
}