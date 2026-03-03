namespace Maliev.SupplierService.Domain.Entities;

public class MaterialCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    
    public ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();
}
