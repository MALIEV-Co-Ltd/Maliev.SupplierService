using System.ComponentModel.DataAnnotations;

namespace Maliev.SupplierService.Api.DTOs.Requests;

/// <summary>
/// Represents a request to create a new contact person for a supplier.
/// </summary>
/// <param name="Name">The full name of the contact person.</param>
/// <param name="Email">The email address of the contact person.</param>
/// <param name="Role">The role or job title of the contact person.</param>
/// <param name="Phone">The phone number of the contact person.</param>
/// <param name="IsPrimary">Indicates if this is the primary contact for the supplier.</param>
public record CreateContactRequest(
    [Required][StringLength(200)] string Name,
    [Required][EmailAddress][StringLength(255)] string Email,
    [StringLength(100)] string? Role,
    [StringLength(50)] string? Phone,
    bool IsPrimary = false
);
