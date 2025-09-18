using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maliev.SupplierService.Data.Entities;

public class Supplier : IAuditable
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(100)]
    public string? RegistrationNumber { get; set; }

    [MaxLength(50)]
    public string? TaxId { get; set; }

    [MaxLength(500)]
    public string? Website { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public SupplierStatus Status { get; set; } = SupplierStatus.Pending;

    // Foreign keys
    public int? CategoryId { get; set; }
    public int? CountryId { get; set; }
    public int? CurrencyId { get; set; }

    // Operational details
    public int? LeadTimeDays { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinimumOrderAmount { get; set; }

    [MaxLength(50)]
    public string? PaymentTerms { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? CreditLimit { get; set; }

    // Performance metrics
    public decimal? QualityRating { get; set; }
    public decimal? DeliveryRating { get; set; }
    public decimal? ServiceRating { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation properties
    public virtual SupplierCategory? Category { get; set; }
    public virtual ICollection<SupplierContact> Contacts { get; set; } = new List<SupplierContact>();
    public virtual ICollection<SupplierAddress> Addresses { get; set; } = new List<SupplierAddress>();
    public virtual ICollection<SupplierDocument> Documents { get; set; } = new List<SupplierDocument>();
    public virtual ICollection<SupplierRating> Ratings { get; set; } = new List<SupplierRating>();
}