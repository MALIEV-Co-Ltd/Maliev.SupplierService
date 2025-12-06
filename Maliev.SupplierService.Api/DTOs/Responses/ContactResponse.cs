namespace Maliev.SupplierService.Api.DTOs.Responses;

/// <summary>
/// Represents a contact person associated with a supplier.
/// </summary>
/// <param name="Id">The unique identifier of the contact.</param>
/// <param name="Name">The full name of the contact person.</param>
/// <param name="Role">The role or job title of the contact person.</param>
/// <param name="Email">The email address of the contact person.</param>
/// <param name="Phone">The phone number of the contact person.</param>
/// <param name="IsPrimary">Indicates whether this contact is the primary contact for the supplier.</param>
/// <param name="CreatedAt">The date and time when the contact record was created.</param>
public record ContactResponse(
    Guid Id,
    string Name,
    string? Role,
    string Email,
    string? Phone,
    bool IsPrimary,
    DateTime CreatedAt
);
