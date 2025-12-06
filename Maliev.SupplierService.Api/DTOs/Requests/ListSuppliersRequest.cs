namespace Maliev.SupplierService.Api.DTOs.Requests;

using Maliev.SupplierService.Data.Enums;

/// <summary>
/// Represents a request to list suppliers with optional filtering, sorting, and pagination.
/// </summary>
/// <param name="Page">The page number for pagination (1-based).</param>
/// <param name="PageSize">The number of items per page.</param>
/// <param name="Status">An optional filter for the supplier's status.</param>
/// <param name="CategoryId">An optional filter for the material category ID.</param>
/// <param name="Capability">An optional filter for a specific supplier capability.</param>
/// <param name="Search">A search term to filter suppliers by name, tax ID, or other fields.</param>
/// <param name="SortBy">The field to sort the results by.</param>
/// <param name="SortOrder">The sort order ('asc' or 'desc').</param>
public record ListSuppliersRequest(
    int Page = 1,
    int PageSize = 20,
    SupplierStatus? Status = null,
    Guid? CategoryId = null,
    string? Capability = null,
    string? Search = null,
    string SortBy = "CompanyName",
    string SortOrder = "asc"
);
