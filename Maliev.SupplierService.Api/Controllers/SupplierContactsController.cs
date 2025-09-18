using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maliev.SupplierService.Api.Models;
using Maliev.SupplierService.Api.Services;

namespace Maliev.SupplierService.Api.Controllers;

[ApiController]
[Route("v{version:apiVersion}/supplier-contacts")]
[Authorize]
public class SupplierContactsController : ControllerBase
{
    private readonly ISupplierContactService _contactService;
    private readonly ILogger<SupplierContactsController> _logger;

    public SupplierContactsController(ISupplierContactService contactService, ILogger<SupplierContactsController> logger)
    {
        _contactService = contactService;
        _logger = logger;
    }

    [HttpGet("supplier/{supplierId}")]
    public async Task<ActionResult<IEnumerable<SupplierContactDto>>> GetContactsBySupplier(int supplierId, CancellationToken cancellationToken = default)
    {
        try
        {
            var contacts = await _contactService.GetContactsBySupplierIdAsync(supplierId, cancellationToken);
            return Ok(contacts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contacts for supplier {SupplierId}", supplierId);
            return StatusCode(500, "An error occurred while retrieving supplier contacts");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierContactDto>> GetContact(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var contact = await _contactService.GetContactByIdAsync(id, cancellationToken);

            if (contact == null)
            {
                return NotFound();
            }

            return Ok(contact);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supplier contact with ID {ContactId}", id);
            return StatusCode(500, "An error occurred while retrieving the supplier contact");
        }
    }

    [HttpPost]
    public async Task<ActionResult<SupplierContactDto>> CreateContact(SupplierContactDto contactDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdContact = await _contactService.CreateContactAsync(contactDto, cancellationToken);
            return CreatedAtAction(nameof(GetContact), new { id = createdContact.Id }, createdContact);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating supplier contact");
            return StatusCode(500, "An error occurred while creating the supplier contact");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SupplierContactDto>> UpdateContact(int id, SupplierContactDto contactDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedContact = await _contactService.UpdateContactAsync(id, contactDto, cancellationToken);

            if (updatedContact == null)
            {
                return NotFound();
            }

            return Ok(updatedContact);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating supplier contact with ID {ContactId}", id);
            return StatusCode(500, "An error occurred while updating the supplier contact");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContact(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _contactService.DeleteContactAsync(id, cancellationToken);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting supplier contact with ID {ContactId}", id);
            return StatusCode(500, "An error occurred while deleting the supplier contact");
        }
    }
}