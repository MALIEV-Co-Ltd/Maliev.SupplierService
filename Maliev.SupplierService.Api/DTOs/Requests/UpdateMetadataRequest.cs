namespace Maliev.SupplierService.Api.DTOs.Requests;

/// <summary>
/// Represents a request to update supplier metadata, typically from external service callbacks.
/// </summary>
/// <param name="LastOrderDate">The date of the supplier's last order.</param>
/// <param name="TotalOrderValue">The total value of all orders from the supplier.</param>
public record UpdateMetadataRequest(
    DateTime? LastOrderDate,
    decimal? TotalOrderValue
);
