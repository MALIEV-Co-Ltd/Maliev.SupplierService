using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maliev.SupplierService.Data.DbContexts;
using Maliev.SupplierService.Data.Entities;
using Maliev.SupplierService.Api.Models;

namespace Maliev.SupplierService.Api.Controllers;

[ApiController]
[Route("v{version:apiVersion}/supplier-categories")]
[Authorize]
public class SupplierCategoriesController : ControllerBase
{
    private readonly SupplierDbContext _context;
    private readonly ILogger<SupplierCategoriesController> _logger;

    public SupplierCategoriesController(SupplierDbContext context, ILogger<SupplierCategoriesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierCategoryDto>>> GetCategories(
        [FromQuery] bool? isActive = null)
    {
        try
        {
            var query = _context.SupplierCategories.AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            var categories = await query
                .OrderBy(c => c.Name)
                .Select(c => new SupplierCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();

            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supplier categories");
            return StatusCode(500, "An error occurred while retrieving supplier categories");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierCategoryDto>> GetCategory(int id)
    {
        try
        {
            var category = await _context.SupplierCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound($"Supplier category with ID {id} not found");
            }

            var categoryDto = new SupplierCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            return Ok(categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supplier category {CategoryId}", id);
            return StatusCode(500, "An error occurred while retrieving the supplier category");
        }
    }

    [HttpPost]
    public async Task<ActionResult<SupplierCategoryDto>> CreateCategory(CreateSupplierCategoryRequest request)
    {
        try
        {
            var category = new SupplierCategory
            {
                Name = request.Name,
                Description = request.Description,
                IsActive = request.IsActive
            };

            _context.SupplierCategories.Add(category);
            await _context.SaveChangesAsync();

            var categoryDto = new SupplierCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating supplier category");
            return StatusCode(500, "An error occurred while creating the supplier category");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, UpdateSupplierCategoryRequest request)
    {
        try
        {
            var category = await _context.SupplierCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound($"Supplier category with ID {id} not found");
            }

            category.Name = request.Name;
            category.Description = request.Description;
            category.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating supplier category {CategoryId}", id);
            return StatusCode(500, "An error occurred while updating the supplier category");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            var category = await _context.SupplierCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound($"Supplier category with ID {id} not found");
            }

            _context.SupplierCategories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting supplier category {CategoryId}", id);
            return StatusCode(500, "An error occurred while deleting the supplier category");
        }
    }
}