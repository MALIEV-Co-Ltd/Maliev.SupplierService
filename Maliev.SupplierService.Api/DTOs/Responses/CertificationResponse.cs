namespace Maliev.SupplierService.Api.DTOs.Responses;

/// <summary>
/// Represents a supplier's certification details.
/// </summary>
/// <param name="Id">The unique identifier of the certification.</param>
/// <param name="DocumentType">The type of the certification document.</param>
/// <param name="DocumentName">The name or title of the certification document.</param>
/// <param name="IssueDate">The date the certification was issued.</param>
/// <param name="ExpirationDate">The optional expiration date of the certification.</param>
/// <param name="ExternalFileRef">An optional external reference or URL to the certification file.</param>
/// <param name="IsExpired">Indicates whether the certification has expired.</param>
/// <param name="IsExpiringSoon">Indicates whether the certification is expiring soon (e.g., within 30 days).</param>
/// <param name="CreatedAt">The date and time when the certification record was created.</param>
public record CertificationResponse(
    Guid Id,
    string DocumentType,
    string DocumentName,
    DateOnly? IssueDate,
    DateOnly? ExpirationDate,
    string? ExternalFileRef,
    bool IsExpired,
    bool IsExpiringSoon,
    DateTime CreatedAt
);
