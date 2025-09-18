using Microsoft.EntityFrameworkCore;
using Maliev.SupplierService.Api.Models;
using Maliev.SupplierService.Data.DbContexts;
using Maliev.SupplierService.Data.Entities;

namespace Maliev.SupplierService.Api.Services
{
    public class SupplierContactService : ISupplierContactService
    {
        private readonly SupplierDbContext _context;
        private readonly ILogger<SupplierContactService> _logger;

        public SupplierContactService(SupplierDbContext context, ILogger<SupplierContactService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<SupplierContactDto>> GetContactsBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default)
        {
            try
            {
                var contacts = await _context.SupplierContacts
                    .Where(c => c.SupplierId == supplierId)
                    .OrderBy(c => c.Role)
                    .ThenBy(c => c.FirstName)
                    .ToListAsync(cancellationToken);

                return contacts.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contacts for supplier {SupplierId}", supplierId);
                throw;
            }
        }

        public async Task<SupplierContactDto?> GetContactByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var contact = await _context.SupplierContacts
                    .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

                return contact != null ? MapToDto(contact) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplier contact with ID {ContactId}", id);
                throw;
            }
        }

        public async Task<SupplierContactDto> CreateContactAsync(SupplierContactDto contactDto, CancellationToken cancellationToken = default)
        {
            try
            {
                var contact = MapToEntity(contactDto);

                _context.SupplierContacts.Add(contact);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Created supplier contact with ID {ContactId}", contact.Id);

                return MapToDto(contact);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supplier contact");
                throw;
            }
        }

        public async Task<SupplierContactDto?> UpdateContactAsync(int id, SupplierContactDto contactDto, CancellationToken cancellationToken = default)
        {
            try
            {
                var existingContact = await _context.SupplierContacts
                    .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

                if (existingContact == null)
                {
                    return null;
                }

                UpdateEntityFromDto(existingContact, contactDto);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Updated supplier contact with ID {ContactId}", id);

                return MapToDto(existingContact);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier contact with ID {ContactId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteContactAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var contact = await _context.SupplierContacts
                    .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

                if (contact == null)
                {
                    return false;
                }

                _context.SupplierContacts.Remove(contact);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Deleted supplier contact with ID {ContactId}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier contact with ID {ContactId}", id);
                throw;
            }
        }

        private static SupplierContactDto MapToDto(SupplierContact contact)
        {
            return new SupplierContactDto
            {
                Id = contact.Id,
                SupplierId = contact.SupplierId,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                Email = contact.Email,
                Phone = contact.Phone,
                Mobile = contact.Mobile,
                JobTitle = contact.JobTitle,
                Department = contact.Department,
                Role = contact.Role,
                IsPrimary = contact.IsPrimary,
                IsActive = contact.IsActive,
                Notes = contact.Notes,
                CreatedAt = contact.CreatedAt,
                UpdatedAt = contact.UpdatedAt
            };
        }

        private static SupplierContact MapToEntity(SupplierContactDto dto)
        {
            return new SupplierContact
            {
                SupplierId = dto.SupplierId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                Mobile = dto.Mobile,
                JobTitle = dto.JobTitle,
                Department = dto.Department,
                Role = dto.Role,
                IsPrimary = dto.IsPrimary,
                IsActive = dto.IsActive,
                Notes = dto.Notes
            };
        }

        private static void UpdateEntityFromDto(SupplierContact entity, SupplierContactDto dto)
        {
            entity.FirstName = dto.FirstName;
            entity.LastName = dto.LastName;
            entity.Email = dto.Email;
            entity.Phone = dto.Phone;
            entity.Mobile = dto.Mobile;
            entity.JobTitle = dto.JobTitle;
            entity.Department = dto.Department;
            entity.Role = dto.Role;
            entity.IsPrimary = dto.IsPrimary;
            entity.IsActive = dto.IsActive;
            entity.Notes = dto.Notes;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}