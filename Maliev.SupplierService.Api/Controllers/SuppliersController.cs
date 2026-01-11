using Asp.Versioning;

using Maliev.SupplierService.Api.DTOs.Requests;
using Maliev.SupplierService.Api.DTOs.Responses;
using Maliev.SupplierService.Api.Constants;
using Maliev.SupplierService.Api.Mapping;
using Maliev.SupplierService.Api.Services;
using Maliev.SupplierService.Data.Entities;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.SupplierService.Api.Controllers;

/// <summary>
/// Main controller for managing supplier information.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("supplier/v{version:apiVersion}/suppliers")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    private readonly ILogger<SuppliersController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SuppliersController"/> class.
    /// </summary>
    /// <param name="supplierService">The supplier service.</param>
    /// <param name="logger">The logger.</param>
    public SuppliersController(
        ISupplierService supplierService,
        ILogger<SuppliersController> logger)
    {
        _supplierService = supplierService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new supplier
    /// </summary>
    [HttpPost]
    [RequirePermission(SupplierPermissions.Suppliers.Create, PreValidateModel = true)]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SupplierResponse>> CreateSupplier(
        [FromBody] CreateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("sub")?.Value ?? "anonymous";
        var userName = User.FindFirst("name")?.Value ?? "Anonymous User";

        var supplier = await _supplierService.CreateAsync(
            request.CompanyName,
            request.TaxId,
            request.Address,
            request.City,
            request.Country,
            request.PostalCode,
            request.MaterialCategoryIds,
            request.Capabilities,
            request.PrimaryContact,
            userId,
            userName,
            cancellationToken);

        var response = supplier.ToSupplierResponse();

        _logger.LogInformation("Created supplier {SupplierId}", supplier.Id);

        return CreatedAtAction(
            nameof(GetSupplier),
            new { id = supplier.Id, version = "1" },
            response);
    }

    /// <summary>
    /// List suppliers with pagination and filters
    /// </summary>
    [HttpGet]
    [RequirePermission(SupplierPermissions.Suppliers.Read, PreValidateModel = true)]
    [ProducesResponseType(typeof(SupplierListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SupplierListResponse>> ListSuppliers(
        [FromQuery] ListSuppliersRequest request,
        CancellationToken cancellationToken = default)
    {
        // Clamp page size
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var page = Math.Max(1, request.Page);

        var (items, totalCount) = await _supplierService.ListSuppliersAsync(
            page, pageSize, request.Status, request.CategoryId, request.Capability, request.Search, request.SortBy, request.SortOrder, cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var response = new SupplierListResponse(
            items.Select(s => s.ToSupplierResponse()).ToList(),
            totalCount,
            page,
            pageSize,
            totalPages);

        return Ok(response);
    }

    /// <summary>
    /// Get supplier details by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(SupplierPermissions.Suppliers.Read, PreValidateModel = true)]
    [ProducesResponseType(typeof(SupplierDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierDetailResponse>> GetSupplier(
        Guid id,
        CancellationToken cancellationToken)
    {
        var supplier = await _supplierService.GetByIdAsync(id, cancellationToken);

        if (supplier is null)
        {
            return NotFound(new { message = $"Supplier with ID {id} not found" });
        }

        var response = supplier.ToSupplierDetailResponse();
        return Ok(response);
    }

    /// <summary>
    /// Validate supplier exists (for service-to-service integration)
    /// </summary>
    [HttpGet("{id:guid}/validate")]
    [RequirePermission(SupplierPermissions.Suppliers.Read, PreValidateModel = true)]
    [ProducesResponseType(typeof(SupplierValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierValidationResponse>> ValidateSupplier(
        Guid id,
        CancellationToken cancellationToken)
    {
        var (isValid, supplier) = await _supplierService.ValidateSupplierAsync(id, cancellationToken);

        if (!isValid || supplier is null)
        {
            return NotFound(new { message = $"Supplier with ID {id} not found" });
        }

        var response = new SupplierValidationResponse(
            supplier.Id,
            supplier.CompanyName,
            supplier.TaxId,
            supplier.Status,
            supplier.Status == Data.Enums.SupplierStatus.Active);

        return Ok(response);
    }

    /// <summary>
    /// Check supplier eligibility for purchase orders
    /// </summary>
    [HttpGet("{id:guid}/eligibility")]
    [RequirePermission(SupplierPermissions.Suppliers.Read, PreValidateModel = true)]
    [ProducesResponseType(typeof(SupplierEligibilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierEligibilityResponse>> CheckEligibility(
        Guid id,
        CancellationToken cancellationToken)
    {
        var (isEligible, reasons) = await _supplierService.CheckEligibilityAsync(id, cancellationToken);

        // If supplier not found, the reason will contain that info
        if (reasons.Count == 1 && reasons[0] == "Supplier not found")
        {
            return NotFound(new { message = $"Supplier with ID {id} not found" });
        }

        var response = new SupplierEligibilityResponse(id, isEligible, reasons);
        return Ok(response);
    }

    /// <summary>
    /// List all material categories
    /// </summary>
    [HttpGet("/supplier/v{version:apiVersion}/suppliers/categories")]
    [RequirePermission(SupplierPermissions.Suppliers.Read, PreValidateModel = true)]
    [ProducesResponseType(typeof(MaterialCategoryListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MaterialCategoryListResponse>> GetCategories(
        CancellationToken cancellationToken)
    {
        var categories = await _supplierService.GetMaterialCategoriesAsync(cancellationToken);

        var response = new MaterialCategoryListResponse(
            categories.Select(c => new MaterialCategoryResponse(c.Id, c.Name, c.Description)).ToList());

        return Ok(response);
    }

    /// <summary>
    /// Update supplier information
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(SupplierPermissions.Suppliers.Update, PreValidateModel = false)]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SupplierResponse>> UpdateSupplier(
        Guid id,
        [FromBody] UpdateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return BadRequest(new { message = $"Manual ModelState check failed: {errors}" });
        }

        var userId = User.FindFirst("sub")?.Value ?? "anonymous";
        var userName = User.FindFirst("name")?.Value ?? "Anonymous User";

        byte[] rowVersion;
        try
        {
            rowVersion = Convert.FromBase64String(request.RowVersion);
        }
        catch (FormatException)
        {
            return BadRequest(new { message = $"Invalid RowVersion format. Must be a Base64 string. Received: '{request.RowVersion}'" });
        }

        try
        {
            var supplier = await _supplierService.UpdateAsync(
                id,
                request.CompanyName,
                request.Address,
                request.City,
                request.Country,
                request.PostalCode,
                request.MaterialCategoryIds,
                request.Capabilities,
                rowVersion,
                userId,
                userName,
                cancellationToken);

            return Ok(supplier.ToSupplierResponse());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "The supplier has been modified by another user. Please refresh and try again." });
        }
    }

    /// <summary>
    /// Update supplier status
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [RequirePermission(SupplierPermissions.Suppliers.Update, PreValidateModel = false)]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierResponse>> UpdateStatus(
        Guid id,
        [FromBody] UpdateStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return BadRequest(new { message = $"Manual ModelState check failed: {errors}" });
        }

        var userId = User.FindFirst("sub")?.Value ?? "anonymous";
        var userName = User.FindFirst("name")?.Value ?? "Anonymous User";

        byte[] rowVersion;
        try
        {
            rowVersion = Convert.FromBase64String(request.RowVersion);
        }
        catch (FormatException)
        {
            return BadRequest(new { message = $"Invalid RowVersion format. Must be a Base64 string. Received: '{request.RowVersion}'" });
        }

        try
        {
            var supplier = await _supplierService.UpdateStatusAsync(
                id,
                request.Status,
                request.Reason,
                rowVersion,
                userId,
                userName,
                cancellationToken);

            return Ok(supplier.ToSupplierResponse());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update supplier metadata (for external service callbacks)
    /// </summary>
    [HttpPatch("{id:guid}/metadata")]
    [RequirePermission(SupplierPermissions.Suppliers.Update, PreValidateModel = false)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMetadata(
        Guid id,
        [FromBody] UpdateMetadataRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _supplierService.UpdateMetadataAsync(
                id,
                request.LastOrderDate,
                request.TotalOrderValue,
                cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete supplier
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(SupplierPermissions.Suppliers.Delete, PreValidateModel = true)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSupplier(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("sub")?.Value ?? "anonymous";
        var userName = User.FindFirst("name")?.Value ?? "Anonymous User";

        try
        {
            await _supplierService.DeleteAsync(id, userId, userName, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
