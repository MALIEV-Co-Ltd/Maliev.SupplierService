namespace Maliev.SupplierService.Api.DTOs.Responses;

/// <summary>
/// Represents the result of a supplier validation.
/// </summary>
/// <param name="Id">The unique identifier of the supplier.</param>
/// <param name="CompanyName">The legal name of the supplier company.</param>
/// <param name="IsActive">Indicates whether the supplier is currently active.</param>
public record SupplierValidationResponse(
    Guid Id,
    string CompanyName,
    bool IsActive
);
