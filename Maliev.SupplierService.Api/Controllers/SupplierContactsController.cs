using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maliev.SupplierService.Data.DbContexts;
using Maliev.SupplierService.Data.Entities;
using Maliev.SupplierService.Api.Models;

namespace Maliev.SupplierService.Api.Controllers;

[ApiController]
[Route("v{version:apiVersion}/suppliers/{supplierId}/contacts")]
[Authorize]
public class SupplierContactsController : ControllerBase
{
    private readonly SupplierDbContext _context;
    private readonly ILogger<SupplierContactsController> _logger;

    public SupplierContactsController(SupplierDbContext context, ILogger<SupplierContactsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierContactDto>>> GetContacts(int supplierId)
    {
        try
        {
            var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == supplierId);
            if (!supplierExists)
            {
                return NotFound($"Supplier with ID {supplierId} not found");
            }

            var contacts = await _context.SupplierContacts
                .Where(c => c.SupplierId == supplierId && c.IsActive)
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .Select(c => new SupplierContactDto
                {
                    Id = c.Id,
                    SupplierId = c.SupplierId,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Email = c.Email,
                    Phone = c.Phone,
                    Mobile = c.Mobile,
                    JobTitle = c.JobTitle,
                    Department = c.Department,
                    Role = c.Role,
                    IsPrimary = c.IsPrimary,
                    IsActive = c.IsActive,
                    Notes = c.Notes,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();

            return Ok(contacts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contacts for supplier {SupplierId}", supplierId);
            return StatusCode(500, "An error occurred while retrieving supplier contacts");
        }
    }

    [HttpGet("{contactId}")]
    public async Task<ActionResult<SupplierContactDto>> GetContact(int supplierId, int contactId)
    {
        try
        {
            var contact = await _context.SupplierContacts
                .FirstOrDefaultAsync(c => c.Id == contactId && c.SupplierId == supplierId);

            if (contact == null)
            {
                return NotFound($"Contact with ID {contactId} not found for supplier {supplierId}");
            }

            var contactDto = new SupplierContactDto
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

            return Ok(contactDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contact {ContactId} for supplier {SupplierId}", contactId, supplierId);
            return StatusCode(500, "An error occurred while retrieving the supplier contact");
        }
    }

    [HttpPost]
    public async Task<ActionResult<SupplierContactDto>> CreateContact(int supplierId, CreateSupplierContactRequest request)
    {
        try
        {
            var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == supplierId);
            if (!supplierExists)
            {
                return NotFound($"Supplier with ID {supplierId} not found");
            }

            var contact = new SupplierContact
            {
                SupplierId = supplierId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                Mobile = request.Mobile,
                JobTitle = request.JobTitle,
                Department = request.Department,
                Role = request.Role,
                IsPrimary = request.IsPrimary,
                IsActive = request.IsActive,
                Notes = request.Notes
            };

            _context.SupplierContacts.Add(contact);
            await _context.SaveChangesAsync();

            var contactDto = new SupplierContactDto
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

            return CreatedAtAction(nameof(GetContact), new { supplierId, contactId = contact.Id }, contactDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contact for supplier {SupplierId}", supplierId);
            return StatusCode(500, "An error occurred while creating the supplier contact");
        }
    }

    [HttpPut("{contactId}")]
    public async Task<IActionResult> UpdateContact(int supplierId, int contactId, UpdateSupplierContactRequest request)
    {
        try
        {
            var contact = await _context.SupplierContacts
                .FirstOrDefaultAsync(c => c.Id == contactId && c.SupplierId == supplierId);

            if (contact == null)
            {
                return NotFound($"Contact with ID {contactId} not found for supplier {supplierId}");
            }

            contact.FirstName = request.FirstName;
            contact.LastName = request.LastName;
            contact.Email = request.Email;
            contact.Phone = request.Phone;
            contact.Mobile = request.Mobile;
            contact.JobTitle = request.JobTitle;
            contact.Department = request.Department;
            contact.Role = request.Role;
            contact.IsPrimary = request.IsPrimary;
            contact.IsActive = request.IsActive;
            contact.Notes = request.Notes;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contact {ContactId} for supplier {SupplierId}", contactId, supplierId);
            return StatusCode(500, "An error occurred while updating the supplier contact");
        }
    }

    [HttpDelete("{contactId}")]
    public async Task<IActionResult> DeleteContact(int supplierId, int contactId)
    {
        try
        {
            var contact = await _context.SupplierContacts
                .FirstOrDefaultAsync(c => c.Id == contactId && c.SupplierId == supplierId);

            if (contact == null)
            {
                return NotFound($"Contact with ID {contactId} not found for supplier {supplierId}");
            }

            _context.SupplierContacts.Remove(contact);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting contact {ContactId} for supplier {SupplierId}", contactId, supplierId);
            return StatusCode(500, "An error occurred while deleting the supplier contact");
        }
    }
}