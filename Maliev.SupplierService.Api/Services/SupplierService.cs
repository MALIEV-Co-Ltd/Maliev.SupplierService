using Microsoft.EntityFrameworkCore;
using Maliev.SupplierService.Api.Models;
using Maliev.SupplierService.Data.DbContexts;
using Maliev.SupplierService.Data.Entities;

namespace Maliev.SupplierService.Api.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly SupplierDbContext _context;
        private readonly ILogger<SupplierService> _logger;

        public SupplierService(SupplierDbContext context, ILogger<SupplierService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<SupplierDto>> GetSuppliersAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            SupplierStatus? status = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _context.Suppliers
                    .Include(s => s.Category)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(s => s.Name.Contains(search) ||
                                           (s.Description != null && s.Description.Contains(search)) ||
                                           (s.Website != null && s.Website.Contains(search)));
                }

                if (status.HasValue)
                {
                    query = query.Where(s => s.Status == status.Value);
                }

                var suppliers = await query
                    .OrderBy(s => s.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                return suppliers.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving suppliers");
                throw;
            }
        }

        public async Task<SupplierDto?> GetSupplierByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var supplier = await _context.Suppliers
                    .Include(s => s.Category)
                    .Include(s => s.Contacts)
                    .Include(s => s.Addresses)
                    .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

                return supplier != null ? MapToDto(supplier) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplier with ID {SupplierId}", id);
                throw;
            }
        }

        public async Task<SupplierDto> CreateSupplierAsync(SupplierDto supplierDto, CancellationToken cancellationToken = default)
        {
            try
            {
                var supplier = MapToEntity(supplierDto);

                _context.Suppliers.Add(supplier);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Created supplier with ID {SupplierId}", supplier.Id);

                return MapToDto(supplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supplier");
                throw;
            }
        }

        public async Task<SupplierDto?> UpdateSupplierAsync(int id, SupplierDto supplierDto, CancellationToken cancellationToken = default)
        {
            try
            {
                var existingSupplier = await _context.Suppliers
                    .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

                if (existingSupplier == null)
                {
                    return null;
                }

                UpdateEntityFromDto(existingSupplier, supplierDto);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Updated supplier with ID {SupplierId}", id);

                return MapToDto(existingSupplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier with ID {SupplierId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteSupplierAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var supplier = await _context.Suppliers
                    .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

                if (supplier == null)
                {
                    return false;
                }

                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Deleted supplier with ID {SupplierId}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier with ID {SupplierId}", id);
                throw;
            }
        }

        public async Task<bool> SupplierExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Suppliers.AnyAsync(s => s.Id == id, cancellationToken);
        }

        private static SupplierDto MapToDto(Supplier supplier)
        {
            return new SupplierDto
            {
                Id = supplier.Id,
                Name = supplier.Name,
                RegistrationNumber = supplier.RegistrationNumber,
                TaxId = supplier.TaxId,
                Website = supplier.Website,
                Description = supplier.Description,
                Status = supplier.Status,
                CategoryId = supplier.CategoryId,
                CountryId = supplier.CountryId,
                CurrencyId = supplier.CurrencyId,
                LeadTimeDays = supplier.LeadTimeDays,
                MinimumOrderAmount = supplier.MinimumOrderAmount,
                PaymentTerms = supplier.PaymentTerms,
                CreditLimit = supplier.CreditLimit,
                QualityRating = supplier.QualityRating,
                DeliveryRating = supplier.DeliveryRating,
                ServiceRating = supplier.ServiceRating,
                CreatedAt = supplier.CreatedAt,
                UpdatedAt = supplier.UpdatedAt,
                Category = supplier.Category != null ? new SupplierCategoryDto
                {
                    Id = supplier.Category.Id,
                    Name = supplier.Category.Name,
                    Description = supplier.Category.Description,
                    IsActive = supplier.Category.IsActive,
                    CreatedAt = supplier.Category.CreatedAt,
                    UpdatedAt = supplier.Category.UpdatedAt
                } : null
            };
        }

        private static Supplier MapToEntity(SupplierDto dto)
        {
            return new Supplier
            {
                Name = dto.Name,
                RegistrationNumber = dto.RegistrationNumber,
                TaxId = dto.TaxId,
                Website = dto.Website,
                Description = dto.Description,
                Status = dto.Status,
                CategoryId = dto.CategoryId,
                CountryId = dto.CountryId,
                CurrencyId = dto.CurrencyId,
                LeadTimeDays = dto.LeadTimeDays,
                MinimumOrderAmount = dto.MinimumOrderAmount,
                PaymentTerms = dto.PaymentTerms,
                CreditLimit = dto.CreditLimit,
                QualityRating = dto.QualityRating,
                DeliveryRating = dto.DeliveryRating,
                ServiceRating = dto.ServiceRating
            };
        }

        private static void UpdateEntityFromDto(Supplier entity, SupplierDto dto)
        {
            entity.Name = dto.Name;
            entity.RegistrationNumber = dto.RegistrationNumber;
            entity.TaxId = dto.TaxId;
            entity.Website = dto.Website;
            entity.Description = dto.Description;
            entity.Status = dto.Status;
            entity.CategoryId = dto.CategoryId;
            entity.CountryId = dto.CountryId;
            entity.CurrencyId = dto.CurrencyId;
            entity.LeadTimeDays = dto.LeadTimeDays;
            entity.MinimumOrderAmount = dto.MinimumOrderAmount;
            entity.PaymentTerms = dto.PaymentTerms;
            entity.CreditLimit = dto.CreditLimit;
            entity.QualityRating = dto.QualityRating;
            entity.DeliveryRating = dto.DeliveryRating;
            entity.ServiceRating = dto.ServiceRating;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}