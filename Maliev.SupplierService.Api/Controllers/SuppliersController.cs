using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maliev.SupplierService.Data.DbContexts;
using Maliev.SupplierService.Data.Entities;
using Maliev.SupplierService.Api.Models;

namespace Maliev.SupplierService.Api.Controllers;

[ApiController]
[Route("v{version:apiVersion}/suppliers")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly SupplierDbContext _context;
    private readonly ILogger<SuppliersController> _logger;

    public SuppliersController(SupplierDbContext context, ILogger<SuppliersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetSuppliers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] SupplierStatus? status = null,
        [FromQuery] int? categoryId = null)
    {
        try
        {
            var query = _context.Suppliers
                .Include(s => s.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => s.Name.Contains(search) ||
                                        s.RegistrationNumber!.Contains(search) ||
                                        s.TaxId!.Contains(search));
            }

            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(s => s.CategoryId == categoryId.Value);
            }

            var totalCount = await query.CountAsync();
            var suppliers = await query
                .OrderBy(s => s.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SupplierDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    RegistrationNumber = s.RegistrationNumber,
                    TaxId = s.TaxId,
                    Website = s.Website,
                    Description = s.Description,
                    Status = s.Status,
                    CategoryId = s.CategoryId,
                    CountryId = s.CountryId,
                    CurrencyId = s.CurrencyId,
                    LeadTimeDays = s.LeadTimeDays,
                    MinimumOrderAmount = s.MinimumOrderAmount,
                    PaymentTerms = s.PaymentTerms,
                    CreditLimit = s.CreditLimit,
                    QualityRating = s.QualityRating,
                    DeliveryRating = s.DeliveryRating,
                    ServiceRating = s.ServiceRating,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    Category = s.Category == null ? null : new SupplierCategoryDto
                    {
                        Id = s.Category.Id,
                        Name = s.Category.Name,
                        Description = s.Category.Description,
                        IsActive = s.Category.IsActive,
                        CreatedAt = s.Category.CreatedAt,
                        UpdatedAt = s.Category.UpdatedAt
                    }
                })
                .ToListAsync();

            Response.Headers["X-Total-Count"] = totalCount.ToString();
            Response.Headers["X-Page"] = page.ToString();
            Response.Headers["X-Page-Size"] = pageSize.ToString();

            return Ok(suppliers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving suppliers");
            return StatusCode(500, "An error occurred while retrieving suppliers");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierDto>> GetSupplier(int id)
    {
        try
        {
            var supplier = await _context.Suppliers
                .Include(s => s.Category)
                .Include(s => s.Contacts.Where(c => c.IsActive))
                .Include(s => s.Addresses.Where(a => a.IsActive))
                .Include(s => s.Documents.Where(d => d.IsActive))
                .Include(s => s.Ratings)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null)
            {
                return NotFound($"Supplier with ID {id} not found");
            }

            var supplierDto = new SupplierDto
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
                Category = supplier.Category == null ? null : new SupplierCategoryDto
                {
                    Id = supplier.Category.Id,
                    Name = supplier.Category.Name,
                    Description = supplier.Category.Description,
                    IsActive = supplier.Category.IsActive,
                    CreatedAt = supplier.Category.CreatedAt,
                    UpdatedAt = supplier.Category.UpdatedAt
                },
                Contacts = supplier.Contacts.Select(c => new SupplierContactDto
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
                }).ToList(),
                Addresses = supplier.Addresses.Select(a => new SupplierAddressDto
                {
                    Id = a.Id,
                    SupplierId = a.SupplierId,
                    Type = a.Type,
                    Building = a.Building,
                    AddressLine1 = a.AddressLine1,
                    AddressLine2 = a.AddressLine2,
                    City = a.City,
                    State = a.State,
                    PostalCode = a.PostalCode,
                    CountryId = a.CountryId,
                    IsPrimary = a.IsPrimary,
                    IsActive = a.IsActive,
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                }).ToList(),
                Documents = supplier.Documents.Select(d => new SupplierDocumentDto
                {
                    Id = d.Id,
                    SupplierId = d.SupplierId,
                    Title = d.Title,
                    Type = d.Type,
                    Description = d.Description,
                    FileName = d.FileName,
                    ContentType = d.ContentType,
                    FileSize = d.FileSize,
                    UploadServiceFileId = d.UploadServiceFileId,
                    ValidFrom = d.ValidFrom,
                    ValidTo = d.ValidTo,
                    IsRequired = d.IsRequired,
                    IsActive = d.IsActive,
                    Notes = d.Notes,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt
                }).ToList(),
                Ratings = supplier.Ratings.Select(r => new SupplierRatingDto
                {
                    Id = r.Id,
                    SupplierId = r.SupplierId,
                    RatingPeriod = r.RatingPeriod,
                    RatingDate = r.RatingDate,
                    QualityRating = r.QualityRating,
                    DeliveryRating = r.DeliveryRating,
                    ServiceRating = r.ServiceRating,
                    PricingRating = r.PricingRating,
                    CommunicationRating = r.CommunicationRating,
                    OverallRating = r.OverallRating,
                    TotalOrders = r.TotalOrders,
                    OnTimeDeliveries = r.OnTimeDeliveries,
                    QualityIssues = r.QualityIssues,
                    Comments = r.Comments,
                    ReviewedBy = r.ReviewedBy,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                }).ToList()
            };

            return Ok(supplierDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supplier {SupplierId}", id);
            return StatusCode(500, "An error occurred while retrieving the supplier");
        }
    }

    [HttpPost]
    public async Task<ActionResult<SupplierDto>> CreateSupplier(CreateSupplierRequest request)
    {
        try
        {
            var supplier = new Supplier
            {
                Name = request.Name,
                RegistrationNumber = request.RegistrationNumber,
                TaxId = request.TaxId,
                Website = request.Website,
                Description = request.Description,
                Status = request.Status,
                CategoryId = request.CategoryId,
                CountryId = request.CountryId,
                CurrencyId = request.CurrencyId,
                LeadTimeDays = request.LeadTimeDays,
                MinimumOrderAmount = request.MinimumOrderAmount,
                PaymentTerms = request.PaymentTerms,
                CreditLimit = request.CreditLimit
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            var supplierDto = new SupplierDto
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
                UpdatedAt = supplier.UpdatedAt
            };

            return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, supplierDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating supplier");
            return StatusCode(500, "An error occurred while creating the supplier");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSupplier(int id, UpdateSupplierRequest request)
    {
        try
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound($"Supplier with ID {id} not found");
            }

            supplier.Name = request.Name;
            supplier.RegistrationNumber = request.RegistrationNumber;
            supplier.TaxId = request.TaxId;
            supplier.Website = request.Website;
            supplier.Description = request.Description;
            supplier.Status = request.Status;
            supplier.CategoryId = request.CategoryId;
            supplier.CountryId = request.CountryId;
            supplier.CurrencyId = request.CurrencyId;
            supplier.LeadTimeDays = request.LeadTimeDays;
            supplier.MinimumOrderAmount = request.MinimumOrderAmount;
            supplier.PaymentTerms = request.PaymentTerms;
            supplier.CreditLimit = request.CreditLimit;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating supplier {SupplierId}", id);
            return StatusCode(500, "An error occurred while updating the supplier");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        try
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound($"Supplier with ID {id} not found");
            }

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting supplier {SupplierId}", id);
            return StatusCode(500, "An error occurred while deleting the supplier");
        }
    }
}