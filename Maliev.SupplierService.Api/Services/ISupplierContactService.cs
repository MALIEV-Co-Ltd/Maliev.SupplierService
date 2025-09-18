using Maliev.SupplierService.Api.Models;

namespace Maliev.SupplierService.Api.Services
{
    public interface ISupplierContactService
    {
        Task<IEnumerable<SupplierContactDto>> GetContactsBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default);

        Task<SupplierContactDto?> GetContactByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<SupplierContactDto> CreateContactAsync(SupplierContactDto contactDto, CancellationToken cancellationToken = default);

        Task<SupplierContactDto?> UpdateContactAsync(int id, SupplierContactDto contactDto, CancellationToken cancellationToken = default);

        Task<bool> DeleteContactAsync(int id, CancellationToken cancellationToken = default);
    }
}