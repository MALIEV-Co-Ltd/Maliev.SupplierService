namespace Maliev.SupplierService.Domain.Entities;

public class SupplierAuditLog
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public string PerformedByName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    
    public Supplier Supplier { get; set; } = null!;
}
