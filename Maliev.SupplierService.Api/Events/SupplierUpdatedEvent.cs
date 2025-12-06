namespace Maliev.SupplierService.Api.Events;

/// <summary>
/// Represents an event that is published when a supplier's information is updated.
/// </summary>
/// <param name="SupplierId">The unique identifier of the updated supplier.</param>
/// <param name="CompanyName">The updated company name of the supplier.</param>
/// <param name="ChangedFields">A read-only list of the names of the fields that were changed.</param>
/// <param name="UpdatedAt">The date and time when the supplier was updated.</param>
/// <param name="UpdatedBy">The ID of the user who updated the supplier.</param>
public record SupplierUpdatedEvent(
    Guid SupplierId,
    string CompanyName,
    IReadOnlyList<string> ChangedFields,
    DateTime UpdatedAt,
    string UpdatedBy
);
