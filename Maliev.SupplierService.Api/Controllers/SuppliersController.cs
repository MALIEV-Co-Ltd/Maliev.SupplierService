using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maliev.SupplierService.Data.Entities;
using Maliev.SupplierService.Api.Models;
using Maliev.SupplierService.Api.Services;

namespace Maliev.SupplierService.Api.Controllers;

[ApiController]
[Route("v{version:apiVersion}/suppliers")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    private readonly ILogger<SuppliersController> _logger;

    public SuppliersController(ISupplierService supplierService, ILogger<SuppliersController> logger)
    {
        _supplierService = supplierService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetSuppliers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] SupplierStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var suppliers = await _supplierService.GetSuppliersAsync(page, pageSize, search, status, cancellationToken);
            return Ok(suppliers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving suppliers");
            return StatusCode(500, "An error occurred while retrieving suppliers");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierDto>> GetSupplier(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var supplier = await _supplierService.GetSupplierByIdAsync(id, cancellationToken);

            if (supplier == null)
            {
                return NotFound();
            }

            return Ok(supplier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supplier with ID {SupplierId}", id);
            return StatusCode(500, "An error occurred while retrieving the supplier");
        }
    }

    [HttpPost]
    public async Task<ActionResult<SupplierDto>> CreateSupplier(SupplierDto supplierDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdSupplier = await _supplierService.CreateSupplierAsync(supplierDto, cancellationToken);
            return CreatedAtAction(nameof(GetSupplier), new { id = createdSupplier.Id }, createdSupplier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating supplier");
            return StatusCode(500, "An error occurred while creating the supplier");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SupplierDto>> UpdateSupplier(int id, SupplierDto supplierDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedSupplier = await _supplierService.UpdateSupplierAsync(id, supplierDto, cancellationToken);

            if (updatedSupplier == null)
            {
                return NotFound();
            }

            return Ok(updatedSupplier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating supplier with ID {SupplierId}", id);
            return StatusCode(500, "An error occurred while updating the supplier");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSupplier(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _supplierService.DeleteSupplierAsync(id, cancellationToken);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting supplier with ID {SupplierId}", id);
            return StatusCode(500, "An error occurred while deleting the supplier");
        }
    }
}