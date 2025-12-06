namespace Maliev.SupplierService.Api.DTOs.Responses;

using Maliev.SupplierService.Data.Enums;

/// <summary>
/// Represents the result of a supplier validation.
/// </summary>
/// <param name="Id">The unique identifier of the supplier.</param>
/// <param name="CompanyName">The legal name of the supplier company.</param>
/// <param name="TaxId">The tax identification number of the supplier.</param>
/// <param name="Status">The current status of the supplier (e.g., Active, Inactive).</param>
/// <param name="IsActive">Indicates whether the supplier is currently active.</param>
public record SupplierValidationResponse(
    Guid Id,
    string CompanyName,
    string TaxId,
    SupplierStatus Status,
    bool IsActive
);
