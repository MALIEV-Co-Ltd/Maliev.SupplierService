using System.ComponentModel.DataAnnotations;

namespace Maliev.SupplierService.Api.DTOs.Requests;

/// <summary>
/// Represents a request to update an existing supplier's information.
/// All properties are optional; only provided values will be updated.
/// </summary>
/// <param name="CompanyName">The updated legal name of the supplier company.</param>
/// <param name="Address">The updated street address of the supplier.</param>
/// <param name="City">The updated city where the supplier is located.</param>
/// <param name="Country">The updated country where the supplier is located.</param>
/// <param name="PostalCode">The updated postal code for the supplier's address.</param>
/// <param name="MaterialCategoryIds">An updated collection of IDs for the material categories the supplier provides.</param>
/// <param name="Capabilities">An updated list of the supplier's capabilities or services.</param>
/// <param name="RowVersion">The required row version for optimistic concurrency control.</param>
public record UpdateSupplierRequest(
    [StringLength(200)] string? CompanyName,
    [StringLength(500)] string? Address,
    [StringLength(100)] string? City,
    [StringLength(100)] string? Country,
    [StringLength(20)] string? PostalCode,
    IEnumerable<Guid>? MaterialCategoryIds,
    IEnumerable<string>? Capabilities,
    [Required] string RowVersion // Required for optimistic concurrency
);
