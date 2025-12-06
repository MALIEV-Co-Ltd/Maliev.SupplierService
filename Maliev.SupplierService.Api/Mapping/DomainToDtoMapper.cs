using Maliev.SupplierService.Api.DTOs.Responses;
using Maliev.SupplierService.Data.Entities;

namespace Maliev.SupplierService.Api.Mapping;

/// <summary>
/// Extension methods for mapping domain entities to DTOs
/// </summary>
public static class DomainToDtoMapper
{
    /// <summary>
    /// Maps a Supplier entity to SupplierResponse DTO
    /// </summary>
    public static SupplierResponse ToSupplierResponse(this Supplier supplier)
    {
        return new SupplierResponse(
            supplier.Id,
            supplier.CompanyName,
            supplier.TaxId,
            supplier.Address,
            supplier.City,
            supplier.Country,
            supplier.PostalCode,
            supplier.Status,
            supplier.OnboardingStage,
            supplier.CreatedAt,
            supplier.UpdatedAt,
            supplier.UpdatedAt.Ticks.ToString());
    }

    /// <summary>
    /// Maps a Supplier entity to SupplierDetailResponse DTO with related collections
    /// </summary>
    public static SupplierDetailResponse ToSupplierDetailResponse(this Supplier supplier)
    {
        return new SupplierDetailResponse(
            supplier.Id,
            supplier.CompanyName,
            supplier.TaxId,
            supplier.Address,
            supplier.City,
            supplier.Country,
            supplier.PostalCode,
            supplier.Status,
            supplier.OnboardingStage,
            supplier.CreatedAt,
            supplier.UpdatedAt,
            supplier.UpdatedAt.Ticks.ToString(),
            supplier.Contacts.Select(c => new ContactResponse(
                c.Id, c.Name, c.Role, c.Email, c.Phone, c.IsPrimary, c.CreatedAt)).ToList(),
            supplier.MaterialCategories.Select(m => new MaterialCategoryResponse(
                m.Id, m.Name, m.Description)).ToList(),
            supplier.Capabilities.Select(c => new CapabilityResponse(
                c.Id, c.Name, c.Description, c.IsActive)).ToList(),
            supplier.Certifications.Select(c => new CertificationResponse(
                c.Id, c.DocumentType.ToString(), c.DocumentName, c.IssueDate, c.ExpirationDate,
                c.ExternalFileRef,
                c.ExpirationDate.HasValue && c.ExpirationDate.Value < DateOnly.FromDateTime(DateTime.UtcNow),
                c.ExpirationDate.HasValue && c.ExpirationDate.Value <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
                c.CreatedAt)).ToList(),
            null // Performance summary computed separately
        );
    }
}
