namespace Maliev.SupplierService.Api.DTOs.Requests;

/// <summary>
/// Represents a request to update an existing contact for a supplier.
/// All properties are optional; only provided values will be updated.
/// </summary>
/// <param name="Name">The updated full name of the contact person.</param>
/// <param name="Email">The updated email address of the contact person.</param>
/// <param name="Role">The updated role or job title of the contact person.</param>
/// <param name="Phone">The updated phone number of the contact person.</param>
/// <param name="IsPrimary">Indicates if this should be set as the primary contact.</param>
public record UpdateContactRequest(
    string? Name,
    string? Email,
    string? Role,
    string? Phone,
    bool? IsPrimary
);
