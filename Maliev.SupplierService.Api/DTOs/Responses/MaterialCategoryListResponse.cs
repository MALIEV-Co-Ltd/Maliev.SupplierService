namespace Maliev.SupplierService.Api.DTOs.Responses;

/// <summary>
/// Represents a list of material categories.
/// </summary>
/// <param name="Categories">A read-only list of material category responses.</param>
public record MaterialCategoryListResponse(
    IReadOnlyList<MaterialCategoryResponse> Categories
);
