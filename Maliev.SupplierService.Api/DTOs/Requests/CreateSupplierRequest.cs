using System.ComponentModel.DataAnnotations;
using Maliev.SupplierService.Application.DTOs.Requests;

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
    [Required][StringLength(200)] string CompanyName,
    [Required][StringLength(50)] string TaxId,
    [Required][StringLength(500)] string Address,
    [Required][StringLength(100)] string City,
    [Required][StringLength(100)] string Country,
    [StringLength(20)] string? PostalCode,
    IEnumerable<Guid>? MaterialCategoryIds,
    IEnumerable<string>? Capabilities,
    CreateContactRequest? PrimaryContact
);
