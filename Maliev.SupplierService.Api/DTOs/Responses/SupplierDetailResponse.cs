namespace Maliev.SupplierService.Api.DTOs.Responses;

using Maliev.SupplierService.Domain.Enums;

/// <summary>
/// Provides comprehensive details about a supplier, including contact information, material categories, capabilities, certifications, and a performance summary.
/// </summary>
/// <param name="Id">The unique identifier of the supplier.</param>
/// <param name="CompanyName">The legal name of the supplier company.</param>
/// <param name="TaxId">The tax identification number of the supplier.</param>
/// <param name="Address">The street address of the supplier.</param>
/// <param name="City">The city where the supplier is located.</param>
/// <param name="Country">The country where the supplier is located.</param>
/// <param name="PostalCode">The postal code for the supplier's address.</param>
/// <param name="Status">The current status of the supplier (e.g., Active, Inactive).</param>
/// <param name="OnboardingStage">The current stage of the supplier's onboarding process.</param>
/// <param name="CreatedAt">The date and time when the supplier record was created.</param>
/// <param name="UpdatedAt">The date and time when the supplier record was last updated.</param>
/// <param name="RowVersion">The row version for optimistic concurrency control.</param>
/// <param name="Contacts">A read-only list of contact persons for the supplier.</param>
/// <param name="MaterialCategories">A read-only list of material categories the supplier provides.</param>
/// <param name="Capabilities">A read-only list of the supplier's capabilities.</param>
/// <param name="Certifications">A read-only list of the supplier's certifications.</param>
/// <param name="PerformanceSummary">A summary of the supplier's performance ratings.</param>
public record SupplierDetailResponse(
    Guid Id,
    string CompanyName,
    string TaxId,
    string Address,
    string City,
    string Country,
    string? PostalCode,
    SupplierStatus Status,
    OnboardingStage OnboardingStage,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string RowVersion,
    IReadOnlyList<ContactResponse> Contacts,
    IReadOnlyList<MaterialCategoryResponse> MaterialCategories,
    IReadOnlyList<CapabilityResponse> Capabilities,
    IReadOnlyList<CertificationResponse> Certifications,
    PerformanceSummaryResponse? PerformanceSummary
);

/// <summary>
/// Summarizes a supplier's performance ratings.
/// </summary>
/// <param name="OverallRating">The overall average rating for the supplier.</param>
/// <param name="QualityRating">The average quality rating for the supplier.</param>
/// <param name="DeliveryRating">The average delivery rating for the supplier.</param>
/// <param name="CommunicationRating">The average communication rating for the supplier.</param>
/// <param name="PricingRating">The average pricing rating for the supplier.</param>
/// <param name="TotalEvaluations">The total number of evaluations contributing to the summary.</param>
/// <param name="LastEvaluationDate">The date of the most recent evaluation.</param>
public record PerformanceSummaryResponse(
    decimal? OverallRating,
    decimal? QualityRating,
    decimal? DeliveryRating,
    decimal? CommunicationRating,
    decimal? PricingRating,
    int TotalEvaluations,
    DateTime? LastEvaluationDate
);
