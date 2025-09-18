using System.ComponentModel.DataAnnotations;

namespace Maliev.SupplierService.Data.Entities;

public class SupplierAddress : IAuditable
{
    public int Id { get; set; }

    public int SupplierId { get; set; }

    public AddressType Type { get; set; }

    [MaxLength(100)]
    public string? Building { get; set; }

    [Required]
    [MaxLength(200)]
    public required string AddressLine1 { get; set; }

    [MaxLength(200)]
    public string? AddressLine2 { get; set; }

    [Required]
    [MaxLength(100)]
    public required string City { get; set; }

    [MaxLength(100)]
    public string? State { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    public int? CountryId { get; set; }

    public bool IsPrimary { get; set; } = false;

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation properties
    public virtual Supplier Supplier { get; set; } = null!;
}