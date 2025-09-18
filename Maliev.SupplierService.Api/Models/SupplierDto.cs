using Maliev.SupplierService.Data.Entities;

namespace Maliev.SupplierService.Api.Models;

public class SupplierDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? TaxId { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public SupplierStatus Status { get; set; }
    public int? CategoryId { get; set; }
    public int? CountryId { get; set; }
    public int? CurrencyId { get; set; }
    public int? LeadTimeDays { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public string? PaymentTerms { get; set; }
    public decimal? CreditLimit { get; set; }
    public decimal? QualityRating { get; set; }
    public decimal? DeliveryRating { get; set; }
    public decimal? ServiceRating { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation DTOs
    public SupplierCategoryDto? Category { get; set; }
    public List<SupplierContactDto> Contacts { get; set; } = new();
    public List<SupplierAddressDto> Addresses { get; set; } = new();
    public List<SupplierDocumentDto> Documents { get; set; } = new();
    public List<SupplierRatingDto> Ratings { get; set; } = new();
}

public class CreateSupplierRequest
{
    public required string Name { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? TaxId { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public SupplierStatus Status { get; set; } = SupplierStatus.Pending;
    public int? CategoryId { get; set; }
    public int? CountryId { get; set; }
    public int? CurrencyId { get; set; }
    public int? LeadTimeDays { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public string? PaymentTerms { get; set; }
    public decimal? CreditLimit { get; set; }
}

public class UpdateSupplierRequest
{
    public required string Name { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? TaxId { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public SupplierStatus Status { get; set; }
    public int? CategoryId { get; set; }
    public int? CountryId { get; set; }
    public int? CurrencyId { get; set; }
    public int? LeadTimeDays { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public string? PaymentTerms { get; set; }
    public decimal? CreditLimit { get; set; }
}