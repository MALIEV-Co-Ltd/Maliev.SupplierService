using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.SupplierService.Api.Constants;
using Maliev.SupplierService.Api.DTOs.Responses;
using Maliev.SupplierService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.SupplierService.Api.Controllers;

/// <summary>
/// Controller for retrieving supplier audit trail information.
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("supplier/v{version:apiVersion}/suppliers/{supplierId:guid}/audit")]
public class SupplierAuditController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SupplierAuditController"/> class.
    /// </summary>
    /// <param name="supplierService">The supplier service.</param>
    public SupplierAuditController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    /// <summary>
    /// Get audit trail for supplier
    /// </summary>
    [HttpGet]
    [RequirePermission(SupplierPermissions.Suppliers.Read)]
    [ProducesResponseType(typeof(AuditLogListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuditLogListResponse>> GetAuditTrail(
        Guid supplierId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);

        var (items, totalCount) = await _supplierService.GetAuditTrailAsync(
            supplierId, startDate, endDate, page, pageSize, cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var response = new AuditLogListResponse(
            items.Select(a => new AuditLogResponse(
                a.Id,
                a.Action,
                a.EntityType,
                a.EntityId,
                a.OldValues,
                a.NewValues,
                a.PerformedBy,
                a.PerformedByName,
                a.Timestamp)).ToList(),
            totalCount,
            page,
            pageSize,
            totalPages);

        return Ok(response);
    }
}
