using Maliev.SupplierService.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maliev.SupplierService.Data.Entities;

/// <summary>
/// Performance evaluation entity for tracking supplier performance ratings
/// </summary>
/// <remarks>
/// Table and column names use snake_case via explicit [Table] and [Column] attributes.
/// Indexes are configured in PerformanceEvaluationConfiguration.
/// Score is constrained to 1-5 range via check constraint.
/// </remarks>
[Table("performance_evaluations")]
public class PerformanceEvaluation
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("supplier_id")]
    public Guid SupplierId { get; set; }

    [Required]
    [Column("rating_category")]
    public PerformanceRatingCategory RatingCategory { get; set; }

    [Required]
    [Column("score")]
    public int Score { get; set; } // 1-5

    [Required]
    [Column("evaluation_date")]
    public DateOnly EvaluationDate { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("evaluator_id")]
    public required string EvaluatorId { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("evaluator_name")]
    public required string EvaluatorName { get; set; }

    [MaxLength(2000)]
    [Column("notes")]
    public string? Notes { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // Navigation property
    [System.Text.Json.Serialization.JsonIgnore] public Supplier Supplier { get; set; } = null!;
}
