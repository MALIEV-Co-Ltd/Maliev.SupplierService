namespace Maliev.SupplierService.Api.DTOs.Responses;

/// <summary>
/// Represents a supplier's eligibility status for certain operations (e.g., participating in purchase orders).
/// </summary>
/// <param name="SupplierId">The unique identifier of the supplier.</param>
/// <param name="IsEligible">Indicates whether the supplier is currently eligible.</param>
/// <param name="Reasons">A read-only list of reasons if the supplier is not eligible.</param>
public record SupplierEligibilityResponse(
    Guid SupplierId,
    bool IsEligible,
    IReadOnlyList<string> Reasons
);
