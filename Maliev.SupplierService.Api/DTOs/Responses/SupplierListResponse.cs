namespace Maliev.SupplierService.Api.DTOs.Responses;

/// <summary>
/// Represents a paginated list of suppliers.
/// </summary>
/// <param name="Items">A read-only list of supplier responses for the current page.</param>
/// <param name="TotalCount">The total number of suppliers available.</param>
/// <param name="Page">The current page number (1-based).</param>
/// <param name="PageSize">The number of items per page.</param>
/// <param name="TotalPages">The total number of pages available.</param>
public record SupplierListResponse(
    IReadOnlyList<SupplierResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
