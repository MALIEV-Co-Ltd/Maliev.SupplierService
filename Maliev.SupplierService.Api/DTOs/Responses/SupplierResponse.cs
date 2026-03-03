using Maliev.SupplierService.Domain.Enums;

namespace Maliev.SupplierService.Api.DTOs.Responses;

/// <summary>
/// Represents a concise view of a supplier's information.
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
public record SupplierResponse(
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
    string RowVersion
);
