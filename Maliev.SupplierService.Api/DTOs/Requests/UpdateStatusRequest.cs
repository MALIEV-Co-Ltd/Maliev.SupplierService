namespace Maliev.SupplierService.Api.DTOs.Requests;

using Maliev.SupplierService.Data.Enums;

/// <summary>
/// Represents a request to update the status of a supplier.
/// </summary>
/// <param name="Status">The new status to apply to the supplier.</param>
/// <param name="Reason">An optional reason for the status change.</param>
public record UpdateStatusRequest(
    SupplierStatus Status,
    string? Reason
);
