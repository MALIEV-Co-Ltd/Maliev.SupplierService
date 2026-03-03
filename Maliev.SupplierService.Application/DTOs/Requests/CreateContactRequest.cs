using System.ComponentModel.DataAnnotations;

namespace Maliev.SupplierService.Application.DTOs.Requests;

/// <summary>
/// Represents a request to create a new contact person for a supplier.
/// </summary>
public record CreateContactRequest(
    [Required][StringLength(200)] string Name,
    [Required][EmailAddress][StringLength(255)] string Email,
    [StringLength(100)] string Role,
    [StringLength(50)] string Phone,
    bool IsPrimary = false
);
