using Maliev.SupplierService.Data.Entities;

namespace Maliev.SupplierService.Api.Models;

public class SupplierDocumentDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public required string Title { get; set; }
    public DocumentType Type { get; set; }
    public string? Description { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
    public string? UploadServiceFileId { get; set; }
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }
    public bool IsRequired { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class CreateSupplierDocumentRequest
{
    public required string Title { get; set; }
    public DocumentType Type { get; set; }
    public string? Description { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
    public string? UploadServiceFileId { get; set; }
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }
    public bool IsRequired { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

public class UpdateSupplierDocumentRequest
{
    public required string Title { get; set; }
    public DocumentType Type { get; set; }
    public string? Description { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
    public string? UploadServiceFileId { get; set; }
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }
    public bool IsRequired { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}