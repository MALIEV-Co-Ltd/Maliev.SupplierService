using Microsoft.EntityFrameworkCore;
using Maliev.SupplierService.Api.Models;
using Maliev.SupplierService.Data.DbContexts;
using Maliev.SupplierService.Data.Entities;

namespace Maliev.SupplierService.Api.Services
{
    public class SupplierCategoryService : ISupplierCategoryService
    {
        private readonly SupplierDbContext _context;
        private readonly ILogger<SupplierCategoryService> _logger;

        public SupplierCategoryService(SupplierDbContext context, ILogger<SupplierCategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<SupplierCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var categories = await _context.SupplierCategories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync(cancellationToken);

                return categories.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplier categories");
                throw;
            }
        }

        public async Task<SupplierCategoryDto?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var category = await _context.SupplierCategories
                    .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

                return category != null ? MapToDto(category) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplier category with ID {CategoryId}", id);
                throw;
            }
        }

        public async Task<SupplierCategoryDto> CreateCategoryAsync(SupplierCategoryDto categoryDto, CancellationToken cancellationToken = default)
        {
            try
            {
                var category = MapToEntity(categoryDto);

                _context.SupplierCategories.Add(category);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Created supplier category with ID {CategoryId}", category.Id);

                return MapToDto(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supplier category");
                throw;
            }
        }

        public async Task<SupplierCategoryDto?> UpdateCategoryAsync(int id, SupplierCategoryDto categoryDto, CancellationToken cancellationToken = default)
        {
            try
            {
                var existingCategory = await _context.SupplierCategories
                    .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

                if (existingCategory == null)
                {
                    return null;
                }

                UpdateEntityFromDto(existingCategory, categoryDto);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Updated supplier category with ID {CategoryId}", id);

                return MapToDto(existingCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier category with ID {CategoryId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var category = await _context.SupplierCategories
                    .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

                if (category == null)
                {
                    return false;
                }

                // Soft delete - mark as inactive
                category.IsActive = false;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Deleted supplier category with ID {CategoryId}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier category with ID {CategoryId}", id);
                throw;
            }
        }

        public async Task<bool> CategoryExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.SupplierCategories.AnyAsync(c => c.Id == id, cancellationToken);
        }

        private static SupplierCategoryDto MapToDto(SupplierCategory category)
        {
            return new SupplierCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }

        private static SupplierCategory MapToEntity(SupplierCategoryDto dto)
        {
            return new SupplierCategory
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = dto.IsActive
            };
        }

        private static void UpdateEntityFromDto(SupplierCategory entity, SupplierCategoryDto dto)
        {
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.IsActive = dto.IsActive;
        }
    }
}