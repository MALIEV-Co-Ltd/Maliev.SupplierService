namespace Maliev.SupplierService.Api.DTOs.Responses;

using Maliev.SupplierService.Domain.Enums;

/// <summary>
/// Represents a single performance evaluation for a supplier.
/// </summary>
/// <param name="Id">The unique identifier of the evaluation.</param>
/// <param name="Category">The category of the performance evaluation.</param>
/// <param name="Score">The score given in the evaluation.</param>
/// <param name="Comments">Comments or feedback for the evaluation.</param>
/// <param name="EvaluationDate">The date the evaluation was conducted.</param>
/// <param name="EvaluatedBy">The ID of the user who performed the evaluation.</param>
/// <param name="EvaluatedByName">The name of the user who performed the evaluation.</param>
/// <param name="CreatedAt">The date and time when the evaluation record was created.</param>
public record EvaluationResponse(
    Guid Id,
    PerformanceRatingCategory Category,
    int Score,
    string? Comments,
    DateOnly EvaluationDate,
    string EvaluatedBy,
    string EvaluatedByName,
    DateTime CreatedAt
);

/// <summary>
/// Represents a paginated list of supplier evaluations.
/// </summary>
/// <param name="Items">A read-only list of evaluation responses.</param>
/// <param name="TotalCount">The total number of evaluations available.</param>
/// <param name="AverageScore">The average score across all evaluations, if available.</param>
public record EvaluationListResponse(
    IReadOnlyList<EvaluationResponse> Items,
    int TotalCount,
    decimal? AverageScore
);
