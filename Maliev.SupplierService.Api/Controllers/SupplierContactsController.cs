using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.SupplierService.Application.Authorization;
using Maliev.SupplierService.Api.DTOs.Responses;
using Maliev.SupplierService.Application.Interfaces;
using Maliev.SupplierService.Application.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.SupplierService.Api.Controllers;

/// <summary>
/// Controller for managing supplier contacts.
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("supplier/v{version:apiVersion}/suppliers/{supplierId:guid}/contacts")]
public class SupplierContactsController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    private readonly ILogger<SupplierContactsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SupplierContactsController"/> class.
    /// </summary>
    /// <param name="supplierService">The supplier service.</param>
    /// <param name="logger">The logger.</param>
    public SupplierContactsController(
        ISupplierService supplierService,
        ILogger<SupplierContactsController> logger)
    {
        _supplierService = supplierService;
        _logger = logger;
    }

    /// <summary>
    /// Add contact to supplier
    /// </summary>
    /// <param name="supplierId">The ID of the supplier to add the contact to.</param>
    /// <param name="request">The contact details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created contact.</returns>
    [HttpPost]
    [RequirePermission(SupplierPermissions.Contacts.Create, PreValidateModel = true)]
    [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactResponse>> AddContact(
        Guid supplierId,
        [FromBody] CreateContactRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("sub")?.Value ?? "anonymous";
        var userName = User.FindFirst("name")?.Value ?? "Anonymous User";

        try
        {
            var contact = await _supplierService.AddContactAsync(
                supplierId,
                request,
                userId,
                userName,
                cancellationToken);

            var response = new ContactResponse(
                contact.Id,
                contact.Name,
                contact.Role,
                contact.Email,
                contact.Phone,
                contact.IsPrimary,
                contact.CreatedAt);

            return CreatedAtAction(
                nameof(GetContacts),
                new { supplierId, version = "1" },
                response);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(new { message = ex.Message });
            }
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error adding contact to supplier {SupplierId}", supplierId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>
    /// List contacts for supplier
    /// </summary>
    /// <param name="supplierId">The ID of the supplier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of contacts for the specified supplier.</returns>
    [HttpGet]
    [RequirePermission(SupplierPermissions.Contacts.Read, PreValidateModel = true)]
    [ProducesResponseType(typeof(IReadOnlyList<ContactResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ContactResponse>>> GetContacts(
        Guid supplierId,
        CancellationToken cancellationToken)
    {
        var (supplier, _) = await _supplierService.GetByIdAsync(supplierId, cancellationToken);
        if (supplier is null)
        {
            return NotFound(new { message = $"Supplier with ID {supplierId} not found" });
        }

        var contacts = supplier.Contacts.Select(c => new ContactResponse(
            c.Id,
            c.Name,
            c.Role,
            c.Email,
            c.Phone,
            c.IsPrimary,
            c.CreatedAt)).ToList();

        return Ok(contacts);
    }
}
