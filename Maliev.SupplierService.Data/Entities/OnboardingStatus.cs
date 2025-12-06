using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Maliev.SupplierService.Data.Enums;

namespace Maliev.SupplierService.Data.Entities;

/// <summary>
/// Onboarding status history entity tracking supplier onboarding stage transitions
/// </summary>
/// <remarks>
/// Table and column names use snake_case via explicit [Table] and [Column] attributes.
/// Indexes are configured in OnboardingStatusConfiguration.
/// </remarks>
[Table("onboarding_statuses")]
public class OnboardingStatus
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("supplier_id")]
    public Guid SupplierId { get; set; }

    [Required]
    [Column("stage")]
    public OnboardingStage Stage { get; set; }

    [Required]
    [Column("transitioned_at")]
    public DateTime TransitionedAt { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("transitioned_by")]
    public required string TransitionedBy { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("transitioned_by_name")]
    public required string TransitionedByName { get; set; }

    [MaxLength(1000)]
    [Column("notes")]
    public string? Notes { get; set; }

    // Navigation property
    public Supplier Supplier { get; set; } = null!;
}
