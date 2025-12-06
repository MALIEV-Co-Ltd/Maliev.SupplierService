using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Maliev.SupplierService.Data.Enums;

namespace Maliev.SupplierService.Data.Entities;

/// <summary>
/// Supplier entity representing vendor/supplier information
/// </summary>
/// <remarks>
/// Table and column names use snake_case via explicit [Table] and [Column] attributes.
/// Indexes are configured in SupplierConfiguration.
/// </remarks>
[Table("suppliers")]
public class Supplier
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("company_name")]
    public required string CompanyName { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("tax_id")]
    public required string TaxId { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("address")]
    public required string Address { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("city")]
    public required string City { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("country")]
    public required string Country { get; set; }

    [MaxLength(20)]
    [Column("postal_code")]
    public string? PostalCode { get; set; }

    [Required]
    [Column("status")]
    public SupplierStatus Status { get; set; } = SupplierStatus.PendingApproval;

    [Required]
    [Column("onboarding_stage")]
    public OnboardingStage OnboardingStage { get; set; } = OnboardingStage.PendingApproval;

    [Column("last_order_date")]
    public DateTime? LastOrderDate { get; set; }

    [Column("total_order_value")]
    public decimal? TotalOrderValue { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<SupplierContact> Contacts { get; set; } = [];
    public ICollection<MaterialCategory> MaterialCategories { get; set; } = [];
    public ICollection<SupplierCapability> Capabilities { get; set; } = [];
    public ICollection<SupplierCertification> Certifications { get; set; } = [];
    public ICollection<PerformanceEvaluation> Evaluations { get; set; } = [];
    public ICollection<OnboardingStatus> OnboardingHistory { get; set; } = [];
    public ICollection<SupplierAuditLog> AuditLogs { get; set; } = [];
}
