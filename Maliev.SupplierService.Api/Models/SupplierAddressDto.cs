using Maliev.SupplierService.Data.Entities;

namespace Maliev.SupplierService.Api.Models;

public class SupplierAddressDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public AddressType Type { get; set; }
    public string? Building { get; set; }
    public required string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public required string City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public int? CountryId { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class CreateSupplierAddressRequest
{
    public AddressType Type { get; set; }
    public string? Building { get; set; }
    public required string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public required string City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public int? CountryId { get; set; }
    public bool IsPrimary { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

public class UpdateSupplierAddressRequest
{
    public AddressType Type { get; set; }
    public string? Building { get; set; }
    public required string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public required string City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public int? CountryId { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}