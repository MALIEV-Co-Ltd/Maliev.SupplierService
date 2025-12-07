using System.ComponentModel.DataAnnotations;
using Maliev.SupplierService.Api.Attributes;
using Maliev.SupplierService.Data.Enums;

namespace Maliev.SupplierService.Api.DTOs.Requests;

/// <summary>
/// Represents a request to create a new performance evaluation for a supplier.
/// </summary>
/// <param name="Category">The category of the performance evaluation.</param>
/// <param name="Score">The score given in the evaluation (e.g., 1-5).</param>
/// <param name="Comments">Optional comments or feedback for the evaluation.</param>
/// <param name="EvaluationDate">The date the evaluation was conducted.</param>
public record CreateEvaluationRequest(
    PerformanceRatingCategory Category,
    [Range(1, 5)] int Score,
    [StringLength(2000)] string? Comments,
    [Required][NotFutureDate] DateOnly EvaluationDate
);
