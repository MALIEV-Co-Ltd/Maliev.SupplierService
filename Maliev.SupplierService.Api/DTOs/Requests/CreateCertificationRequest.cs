using Maliev.SupplierService.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Maliev.SupplierService.Api.DTOs.Requests;

/// <summary>
/// Represents a request to create a new certification for a supplier.
/// </summary>
/// <param name="DocumentType">The type of certification document.</param>
/// <param name="DocumentName">The name or title of the certification document.</param>
/// <param name="IssueDate">The date the certification was issued.</param>
/// <param name="ExpirationDate">The optional expiration date of the certification.</param>
/// <param name="ExternalFileRef">An optional external reference or URL to the certification file.</param>
/// <param name="Notes">Optional notes or comments about the certification.</param>
public record CreateCertificationRequest(
    CertificationType DocumentType,
    [Required][StringLength(255)] string DocumentName,
    [Required] DateOnly IssueDate,
    DateOnly? ExpirationDate,
    [StringLength(500)] string? ExternalFileRef,
    [StringLength(2000)] string? Notes
);
