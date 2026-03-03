using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.SupplierService.Api.Constants;
using Maliev.SupplierService.Api.DTOs.Requests;
using Maliev.SupplierService.Api.DTOs.Responses;
using Maliev.SupplierService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.SupplierService.Api.Controllers;

/// <summary>
/// Controller for managing supplier certifications.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("supplier/v{version:apiVersion}/suppliers/{supplierId:guid}/certifications")]
public class SupplierCertificationsController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    private readonly ILogger<SupplierCertificationsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SupplierCertificationsController"/> class.
    /// </summary>
    /// <param name="supplierService">The supplier service.</param>
    /// <param name="logger">The logger.</param>
    public SupplierCertificationsController(
        ISupplierService supplierService,
        ILogger<SupplierCertificationsController> logger)
    {
        _supplierService = supplierService;
        _logger = logger;
    }

    /// <summary>
    /// Add certification to supplier
    /// </summary>
    /// <param name="supplierId">The ID of the supplier to add the certification to.</param>
    /// <param name="request">The certification details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created certification.</returns>
    [HttpPost]
    [RequirePermission(SupplierPermissions.Suppliers.Update, PreValidateModel = true)]
    [ProducesResponseType(typeof(CertificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CertificationResponse>> AddCertification(
        Guid supplierId,
        [FromBody] CreateCertificationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("sub")?.Value ?? "anonymous";
        var userName = User.FindFirst("name")?.Value ?? "Anonymous User";

        try
        {
            var certification = await _supplierService.AddCertificationAsync(
                supplierId,
                request.DocumentType,
                request.DocumentName,
                request.IssueDate,
                request.ExpirationDate,
                request.ExternalFileRef,
                request.Notes,
                userId,
                userName,
                cancellationToken);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var response = new CertificationResponse(
                certification.Id,
                certification.DocumentType.ToString(),
                certification.DocumentName,
                certification.IssueDate,
                certification.ExpirationDate,
                certification.ExternalFileRef,
                certification.ExpirationDate.HasValue && certification.ExpirationDate.Value < today,
                certification.ExpirationDate.HasValue && certification.ExpirationDate.Value <= today.AddDays(30),
                DateTime.UtcNow);

            return CreatedAtAction(
                nameof(AddCertification),
                new { supplierId, id = certification.Id, version = "1" },
                response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete certification from supplier
    /// </summary>
    /// <param name="supplierId">The ID of the supplier.</param>
    /// <param name="certificationId">The ID of the certification to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty response if successful.</returns>
    [HttpDelete("{certificationId:guid}")]
    [RequirePermission(SupplierPermissions.Suppliers.Update, PreValidateModel = true)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCertification(
        Guid supplierId,
        Guid certificationId,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("sub")?.Value ?? "anonymous";
        var userName = User.FindFirst("name")?.Value ?? "Anonymous User";

        try
        {
            await _supplierService.DeleteCertificationAsync(
                supplierId,
                certificationId,
                userId,
                userName,
                cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

/// <summary>
/// Controller for handling certifications across all suppliers.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("supplier/v{version:apiVersion}/certifications")]
public class CertificationsController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CertificationsController"/> class.
    /// </summary>
    /// <param name="supplierService">The supplier service.</param>
    public CertificationsController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    /// <summary>
    /// Get certifications expiring within threshold days
    /// </summary>
    [HttpGet("expiring")]
    [RequirePermission(SupplierPermissions.Suppliers.Read, PreValidateModel = true)]
    [ProducesResponseType(typeof(ExpiringCertificationsListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ExpiringCertificationsListResponse>> GetExpiringCertifications(
        [FromQuery] int days = 30,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        days = Math.Clamp(days, 1, 365);
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);

        var (items, totalCount) = await _supplierService.GetExpiringCertificationsAsync(days, page, pageSize, cancellationToken);

        var responseItems = items.Select(c => new ExpiringCertificationResponse(
            c.Certification.Id,
            c.Supplier.Id,
            c.Supplier.CompanyName,
            c.Certification.DocumentType.ToString(),
            c.Certification.DocumentName,
            c.Certification.ExpirationDate!.Value,
            c.DaysUntilExpiration)).ToList();

        return Ok(new ExpiringCertificationsListResponse(responseItems, totalCount));
    }
}
