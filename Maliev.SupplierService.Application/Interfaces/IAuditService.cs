using Maliev.SupplierService.Domain.Entities;

namespace Maliev.SupplierService.Application.Interfaces;

public interface IAuditService
{
    void LogChange(
        ISupplierDbContext context,
        Guid supplierId,
        string action,
        string entityType,
        Guid entityId,
        object? oldValues,
        object? newValues,
        string performedBy,
        string performedByName);
}
