namespace Maliev.SupplierService.Api.DTOs.Responses;

/// <summary>
/// Represents a supplier's capability or service.
/// </summary>
/// <param name="Id">The unique identifier of the capability.</param>
/// <param name="Name">The name of the capability.</param>
/// <param name="Description">An optional description of the capability.</param>
/// <param name="IsActive">Indicates whether the capability is active.</param>
public record CapabilityResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive
);
