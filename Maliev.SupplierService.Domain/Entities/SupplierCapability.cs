namespace Maliev.SupplierService.Domain.Entities;

public class SupplierCapability
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public Supplier Supplier { get; set; } = null!;
}
