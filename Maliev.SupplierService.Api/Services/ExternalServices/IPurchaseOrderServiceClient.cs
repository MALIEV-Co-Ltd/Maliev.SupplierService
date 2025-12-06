namespace Maliev.SupplierService.Api.Services.ExternalServices;

/// <summary>
/// Defines the contract for interacting with the external Purchase Order Service.
/// </summary>
public interface IPurchaseOrderServiceClient
{
    /// <summary>
    /// Checks if there are any existing purchase orders referencing the specified supplier.
    /// </summary>
    /// <param name="supplierId">The unique identifier of the supplier to check.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="DependencyCheckResult"/> indicating whether references exist and any associated messages.</returns>
    Task<DependencyCheckResult> CheckReferencesAsync(Guid supplierId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the result of a dependency check against an external service.
/// </summary>
/// <param name="HasReferences">Indicates whether the supplier has existing references in the external service.</param>
/// <param name="ServiceName">The name of the external service that was checked.</param>
/// <param name="ReferenceCount">The number of references found in the external service.</param>
/// <param name="ErrorMessage">An optional error message if the dependency check failed or encountered an issue.</param>
/// <param name="ServiceUnavailable">Indicates whether the external service was unavailable or unreachable.</param>
public record DependencyCheckResult(
    bool HasReferences,
    string ServiceName,
    int ReferenceCount,
    string? ErrorMessage,
    bool ServiceUnavailable);
