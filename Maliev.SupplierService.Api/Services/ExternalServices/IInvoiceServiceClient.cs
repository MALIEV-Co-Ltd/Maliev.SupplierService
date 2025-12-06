namespace Maliev.SupplierService.Api.Services.ExternalServices;

/// <summary>
/// Defines the contract for interacting with the external Invoice Service.
/// </summary>
public interface IInvoiceServiceClient
{
    /// <summary>
    /// Checks if there are any existing invoices referencing the specified supplier.
    /// </summary>
    /// <param name="supplierId">The unique identifier of the supplier to check.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="DependencyCheckResult"/> indicating whether references exist and any associated messages.</returns>
    Task<DependencyCheckResult> CheckReferencesAsync(Guid supplierId, CancellationToken cancellationToken = default);
}
