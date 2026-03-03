using Maliev.SupplierService.Domain.Enums;

namespace Maliev.SupplierService.Domain.Entities;

public class SupplierCertification
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public CertificationType DocumentType { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public DateOnly IssueDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public string? ExternalFileRef { get; set; }
    public string? Notes { get; set; }

    public Supplier Supplier { get; set; } = null!;
}
