namespace Maliev.SupplierService.Api.Services.ExternalServices;

public interface IMaterialServiceClient
{
    Task<DependencyCheckResult> CheckReferencesAsync(Guid supplierId, CancellationToken cancellationToken = default);
}
