namespace Maliev.SupplierService.Api.DTOs.Responses;

/// <summary>
/// Represents an error response related to service dependencies.
/// </summary>
/// <param name="Message">A general message describing the dependency error.</param>
/// <param name="Dependencies">A read-only list of specific dependency information.</param>
public record DependencyErrorResponse(
    string Message,
    IReadOnlyList<DependencyInfo> Dependencies);

/// <summary>
/// Provides detailed information about a single service dependency.
/// </summary>
/// <param name="ServiceName">The name of the dependent service.</param>
/// <param name="ReferenceCount">The number of references to this dependency.</param>
/// <param name="ErrorMessage">An optional error message specific to this dependency.</param>
public record DependencyInfo(
    string ServiceName,
    int ReferenceCount,
    string? ErrorMessage = null);
