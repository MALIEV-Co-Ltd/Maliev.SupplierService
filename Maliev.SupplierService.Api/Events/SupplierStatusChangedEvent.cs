namespace Maliev.SupplierService.Api.Events;

using Maliev.SupplierService.Data.Enums;

/// <summary>
/// Represents an event that is published when a supplier's status changes.
/// </summary>
/// <param name="SupplierId">The unique identifier of the supplier whose status changed.</param>
/// <param name="OldStatus">The previous status of the supplier.</param>
/// <param name="NewStatus">The new status of the supplier.</param>
/// <param name="ChangedAt">The date and time when the status change occurred.</param>
/// <param name="ChangedBy">The ID of the user who changed the supplier's status.</param>
public record SupplierStatusChangedEvent(
    Guid SupplierId,
    SupplierStatus OldStatus,
    SupplierStatus NewStatus,
    DateTime ChangedAt,
    string ChangedBy
);
