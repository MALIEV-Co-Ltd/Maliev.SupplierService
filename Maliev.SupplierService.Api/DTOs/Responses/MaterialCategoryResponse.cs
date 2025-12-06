namespace Maliev.SupplierService.Api.DTOs.Responses;

/// <summary>
/// Represents a material category.
/// </summary>
/// <param name="Id">The unique identifier of the material category.</param>
/// <param name="Name">The name of the material category.</param>
/// <param name="Description">An optional description of the material category.</param>
public record MaterialCategoryResponse(
    Guid Id,
    string Name,
    string? Description
);
