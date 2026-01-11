using System.ComponentModel.DataAnnotations;
using Maliev.SupplierService.Data.Enums;

namespace Maliev.SupplierService.Api.DTOs.Requests;

/// <summary>
/// Represents a request to update the status of a supplier.
/// </summary>
/// <param name="Status">The new status to apply to the supplier.</param>
/// <param name="Reason">An optional reason for the status change.</param>
/// <param name="RowVersion">The required row version for optimistic concurrency control.</param>
public record UpdateStatusRequest(
    SupplierStatus Status,
    string? Reason,
    [Required] string RowVersion
);
