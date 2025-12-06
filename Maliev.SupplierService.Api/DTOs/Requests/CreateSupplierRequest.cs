namespace Maliev.SupplierService.Api.DTOs.Requests;

/// <summary>
/// Represents a request to create a new supplier.
/// </summary>
/// <param name="CompanyName">The legal name of the supplier company.</param>
/// <param name="TaxId">The tax identification number of the supplier.</param>
/// <param name="Address">The street address of the supplier.</param>
/// <param name="City">The city where the supplier is located.</param>
/// <param name="Country">The country where the supplier is located.</param>
/// <param name="PostalCode">The postal code for the supplier's address.</param>
/// <param name="MaterialCategoryIds">A collection of IDs for the material categories the supplier provides.</param>
/// <param name="Capabilities">A list of the supplier's capabilities or services.</param>
/// <param name="PrimaryContact">The primary contact person for the supplier.</param>
public record CreateSupplierRequest(
    string CompanyName,
    string TaxId,
    string Address,
    string City,
    string Country,
    string? PostalCode,
    IEnumerable<Guid>? MaterialCategoryIds,
    IEnumerable<string>? Capabilities,
    CreateContactRequest? PrimaryContact
);
