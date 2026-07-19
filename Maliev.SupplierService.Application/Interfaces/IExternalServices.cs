namespace Maliev.SupplierService.Application.Interfaces;

public interface IPurchaseOrderServiceClient
{
    Task<(bool HasReferences, bool ServiceUnavailable)> CheckReferencesAsync(Guid supplierId, CancellationToken cancellationToken);
}

public interface IInvoiceServiceClient
{
    Task<(bool HasReferences, bool ServiceUnavailable)> CheckReferencesAsync(Guid supplierId, CancellationToken cancellationToken);
}

public interface IMaterialServiceClient
{
    Task<(bool HasReferences, bool ServiceUnavailable)> CheckReferencesAsync(Guid supplierId, CancellationToken cancellationToken);
}
