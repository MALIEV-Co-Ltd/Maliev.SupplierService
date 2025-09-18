using Maliev.SupplierService.Api.Models;
using Maliev.SupplierService.Data.Entities;

namespace Maliev.SupplierService.Api.Services
{
    public interface ISupplierService
    {
        Task<IEnumerable<SupplierDto>> GetSuppliersAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            SupplierStatus? status = null,
            CancellationToken cancellationToken = default);

        Task<SupplierDto?> GetSupplierByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<SupplierDto> CreateSupplierAsync(SupplierDto supplierDto, CancellationToken cancellationToken = default);

        Task<SupplierDto?> UpdateSupplierAsync(int id, SupplierDto supplierDto, CancellationToken cancellationToken = default);

        Task<bool> DeleteSupplierAsync(int id, CancellationToken cancellationToken = default);

        Task<bool> SupplierExistsAsync(int id, CancellationToken cancellationToken = default);
    }
}