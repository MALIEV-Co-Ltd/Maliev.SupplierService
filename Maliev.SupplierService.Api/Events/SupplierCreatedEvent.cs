namespace Maliev.SupplierService.Api.Events;

/// <summary>
/// Represents an event that is published when a new supplier is created.
/// </summary>
/// <param name="SupplierId">The unique identifier of the newly created supplier.</param>
/// <param name="CompanyName">The company name of the new supplier.</param>
/// <param name="TaxId">The tax identification number of the new supplier.</param>
/// <param name="Country">The country of the new supplier.</param>
/// <param name="CreatedAt">The date and time when the supplier was created.</param>
/// <param name="CreatedBy">The ID of the user who created the supplier.</param>
public record SupplierCreatedEvent(
    Guid SupplierId,
    string CompanyName,
    string TaxId,
    string Country,
    DateTime CreatedAt,
    string CreatedBy
);
