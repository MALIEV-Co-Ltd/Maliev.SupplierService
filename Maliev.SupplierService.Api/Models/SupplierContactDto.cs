using Maliev.SupplierService.Data.Entities;

namespace Maliev.SupplierService.Api.Models;

public class SupplierContactDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public ContactRole Role { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class CreateSupplierContactRequest
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public ContactRole Role { get; set; } = ContactRole.Primary;
    public bool IsPrimary { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

public class UpdateSupplierContactRequest
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public ContactRole Role { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}