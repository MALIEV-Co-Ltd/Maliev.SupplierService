using Maliev.SupplierService.Api.DTOs.Responses;
using Maliev.SupplierService.Domain.Entities;
using Maliev.SupplierService.Domain.Enums;

namespace Maliev.SupplierService.Api.Mapping;

/// <summary>
/// Extension methods for mapping domain entities to DTOs
/// </summary>
public static class DomainToDtoMapper
{
    /// <summary>
    /// Maps a Supplier entity to SupplierResponse DTO
    /// </summary>
    public static SupplierResponse ToSupplierResponse(this Supplier supplier, uint xmin)
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
            xmin.ToString());
    }

    /// <summary>
    /// Maps a Supplier entity to SupplierDetailResponse DTO with related collections
    /// </summary>
    public static SupplierDetailResponse ToSupplierDetailResponse(this Supplier supplier, uint xmin)
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
            xmin.ToString(),
            supplier.Contacts.Select(c => new ContactResponse(
                c.Id, c.Name, c.Role, c.Email, c.Phone, c.IsPrimary, c.CreatedAt)).ToList(),
            supplier.MaterialCategories.Select(m => new MaterialCategoryResponse(
                m.Id, m.Name, string.Empty)).ToList(), // Domain entity missing Description
            supplier.Capabilities.Select(c => new CapabilityResponse(
                c.Id, c.Name, string.Empty, c.IsActive)).ToList(), // Domain entity missing Description
            supplier.Certifications.Select(c => new CertificationResponse(
                c.Id, c.DocumentType.ToString(), c.DocumentName, c.IssueDate, c.ExpirationDate,
                c.ExternalFileRef,
                c.ExpirationDate.HasValue && c.ExpirationDate.Value < DateOnly.FromDateTime(DateTime.UtcNow),
                c.ExpirationDate.HasValue && c.ExpirationDate.Value <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
                DateTime.UtcNow)).ToList(), // SupplierCertification missing CreatedAt, using UtcNow as placeholder
            supplier.Evaluations.Any() ? new PerformanceSummaryResponse(
                (decimal)supplier.Evaluations.Average(e => e.Score),
                (decimal?)supplier.Evaluations.Where(e => e.RatingCategory == PerformanceRatingCategory.Quality).Select(e => (int?)e.Score).Average(),
                (decimal?)supplier.Evaluations.Where(e => e.RatingCategory == PerformanceRatingCategory.Delivery).Select(e => (int?)e.Score).Average(),
                (decimal?)supplier.Evaluations.Where(e => e.RatingCategory == PerformanceRatingCategory.Service).Select(e => (int?)e.Score).Average(),
                (decimal?)supplier.Evaluations.Where(e => e.RatingCategory == PerformanceRatingCategory.Cost).Select(e => (int?)e.Score).Average(),
                supplier.Evaluations.Count,
                supplier.Evaluations.Max(e => e.EvaluationDate).ToDateTime(TimeOnly.MinValue)) : null
        );
    }
}
