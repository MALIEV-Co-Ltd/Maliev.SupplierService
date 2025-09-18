namespace Maliev.SupplierService.Api.Models;

public class SupplierCategoryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class CreateSupplierCategoryRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateSupplierCategoryRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}