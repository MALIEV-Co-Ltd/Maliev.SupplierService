namespace Maliev.SupplierService.Api.DTOs.Responses;

/// <summary>
/// Represents the minimal supplier projection exposed to dependent services.
/// </summary>
/// <param name="Id">The unique identifier of the supplier.</param>
/// <param name="CompanyName">The legal name of the supplier company.</param>
/// <param name="IsActive">Indicates whether the supplier is currently active.</param>
public record SupplierReferenceResponse(
    Guid Id,
    string CompanyName,
    bool IsActive
);
