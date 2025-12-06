namespace Maliev.SupplierService.Api.DTOs.Responses;

/// <summary>
/// Represents a supplier certification that is expiring soon.
/// </summary>
/// <param name="CertificationId">The unique identifier of the certification.</param>
/// <param name="SupplierId">The unique identifier of the supplier associated with the certification.</param>
/// <param name="SupplierName">The name of the supplier.</param>
/// <param name="DocumentType">The type of the certification document.</param>
/// <param name="DocumentName">The name or title of the certification document.</param>
/// <param name="ExpirationDate">The expiration date of the certification.</param>
/// <param name="DaysUntilExpiration">The number of days remaining until the certification expires.</param>
public record ExpiringCertificationResponse(
    Guid CertificationId,
    Guid SupplierId,
    string SupplierName,
    string DocumentType,
    string DocumentName,
    DateOnly ExpirationDate,
    int DaysUntilExpiration
);

/// <summary>
/// Represents a paginated list of expiring supplier certifications.
/// </summary>
/// <param name="Items">A read-only list of expiring certification responses.</param>
/// <param name="TotalCount">The total number of expiring certifications found.</param>
public record ExpiringCertificationsListResponse(
    IReadOnlyList<ExpiringCertificationResponse> Items,
    int TotalCount
);
