using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maliev.SupplierService.Data.Entities;

public class SupplierRating : IAuditable
{
    public int Id { get; set; }

    public int SupplierId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string RatingPeriod { get; set; }

    public DateTimeOffset RatingDate { get; set; }

    // Rating metrics (1-5 scale)
    [Column(TypeName = "decimal(3,2)")]
    public decimal QualityRating { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal DeliveryRating { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal ServiceRating { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal PricingRating { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal CommunicationRating { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal OverallRating { get; set; }

    // Performance metrics
    public int? TotalOrders { get; set; }
    public int? OnTimeDeliveries { get; set; }
    public int? QualityIssues { get; set; }

    [MaxLength(1000)]
    public string? Comments { get; set; }

    [MaxLength(100)]
    public string? ReviewedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation properties
    public virtual Supplier Supplier { get; set; } = null!;
}