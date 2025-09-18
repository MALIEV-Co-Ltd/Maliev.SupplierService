using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maliev.SupplierService.Api.Models;
using Maliev.SupplierService.Api.Services;

namespace Maliev.SupplierService.Api.Controllers;

[ApiController]
[Route("v{version:apiVersion}/supplier-categories")]
[Authorize]
public class SupplierCategoriesController : ControllerBase
{
    private readonly ISupplierCategoryService _categoryService;
    private readonly ILogger<SupplierCategoriesController> _logger;

    public SupplierCategoriesController(ISupplierCategoryService categoryService, ILogger<SupplierCategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierCategoryDto>>> GetCategories(CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _categoryService.GetCategoriesAsync(cancellationToken);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supplier categories");
            return StatusCode(500, "An error occurred while retrieving supplier categories");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierCategoryDto>> GetCategory(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id, cancellationToken);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supplier category with ID {CategoryId}", id);
            return StatusCode(500, "An error occurred while retrieving the supplier category");
        }
    }

    [HttpPost]
    public async Task<ActionResult<SupplierCategoryDto>> CreateCategory(SupplierCategoryDto categoryDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdCategory = await _categoryService.CreateCategoryAsync(categoryDto, cancellationToken);
            return CreatedAtAction(nameof(GetCategory), new { id = createdCategory.Id }, createdCategory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating supplier category");
            return StatusCode(500, "An error occurred while creating the supplier category");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SupplierCategoryDto>> UpdateCategory(int id, SupplierCategoryDto categoryDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedCategory = await _categoryService.UpdateCategoryAsync(id, categoryDto, cancellationToken);

            if (updatedCategory == null)
            {
                return NotFound();
            }

            return Ok(updatedCategory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating supplier category with ID {CategoryId}", id);
            return StatusCode(500, "An error occurred while updating the supplier category");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _categoryService.DeleteCategoryAsync(id, cancellationToken);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting supplier category with ID {CategoryId}", id);
            return StatusCode(500, "An error occurred while deleting the supplier category");
        }
    }
}