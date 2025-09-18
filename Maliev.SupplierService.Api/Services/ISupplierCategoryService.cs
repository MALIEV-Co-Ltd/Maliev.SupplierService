using Maliev.SupplierService.Api.Models;

namespace Maliev.SupplierService.Api.Services
{
    public interface ISupplierCategoryService
    {
        Task<IEnumerable<SupplierCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);

        Task<SupplierCategoryDto?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<SupplierCategoryDto> CreateCategoryAsync(SupplierCategoryDto categoryDto, CancellationToken cancellationToken = default);

        Task<SupplierCategoryDto?> UpdateCategoryAsync(int id, SupplierCategoryDto categoryDto, CancellationToken cancellationToken = default);

        Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);

        Task<bool> CategoryExistsAsync(int id, CancellationToken cancellationToken = default);
    }
}