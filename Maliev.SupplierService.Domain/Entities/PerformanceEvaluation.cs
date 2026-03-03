using Maliev.SupplierService.Domain.Enums;

namespace Maliev.SupplierService.Domain.Entities;

public class PerformanceEvaluation
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public PerformanceRatingCategory RatingCategory { get; set; }
    public int Score { get; set; }
    public string? Notes { get; set; }
    public DateOnly EvaluationDate { get; set; }
    public string EvaluatorId { get; set; } = string.Empty;
    public string EvaluatorName { get; set; } = string.Empty;

    public Supplier Supplier { get; set; } = null!;
}
