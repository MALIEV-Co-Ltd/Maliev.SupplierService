using Asp.Versioning;
using Maliev.SupplierService.Api.DTOs.Requests;
using Maliev.SupplierService.Api.DTOs.Responses;
using Maliev.SupplierService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.SupplierService.Api.Controllers;

/// <summary>
/// Controller for managing supplier performance evaluations.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("supplier/v{version:apiVersion}/suppliers/{supplierId:guid}/evaluations")]
[Authorize]
public class SupplierEvaluationsController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    private readonly ILogger<SupplierEvaluationsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SupplierEvaluationsController"/> class.
    /// </summary>
    /// <param name="supplierService">The supplier service.</param>
    /// <param name="logger">The logger.</param>
    public SupplierEvaluationsController(
        ISupplierService supplierService,
        ILogger<SupplierEvaluationsController> logger)
    {
        _supplierService = supplierService;
        _logger = logger;
    }

    /// <summary>
    /// Add performance evaluation to supplier
    /// </summary>
    /// <param name="supplierId">The ID of the supplier to add the evaluation to.</param>
    /// <param name="request">The evaluation details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created evaluation.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(EvaluationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EvaluationResponse>> AddEvaluation(
        Guid supplierId,
        [FromBody] CreateEvaluationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("sub")?.Value ?? "anonymous";
        var userName = User.FindFirst("name")?.Value ?? "Anonymous User";

        try
        {
            var evaluation = await _supplierService.AddEvaluationAsync(
                supplierId,
                request.Category,
                request.Score,
                request.Comments,
                request.EvaluationDate,
                userId,
                userName,
                cancellationToken);

            var response = new EvaluationResponse(
                evaluation.Id,
                evaluation.RatingCategory,
                evaluation.Score,
                evaluation.Notes,
                evaluation.EvaluationDate,
                evaluation.EvaluatorId,
                evaluation.EvaluatorName,
                evaluation.CreatedAt);

            return CreatedAtAction(
                nameof(GetEvaluations),
                new { supplierId, version = "1" },
                response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all evaluations for supplier
    /// </summary>
    /// <param name="supplierId">The ID of the supplier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of evaluations for the specified supplier.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(EvaluationListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EvaluationListResponse>> GetEvaluations(
        Guid supplierId,
        CancellationToken cancellationToken)
    {
        var evaluations = await _supplierService.GetEvaluationsAsync(supplierId, cancellationToken);

        var items = evaluations.Select(e => new EvaluationResponse(
            e.Id,
            e.RatingCategory,
            e.Score,
            e.Notes,
            e.EvaluationDate,
            e.EvaluatorId,
            e.EvaluatorName,
            e.CreatedAt)).ToList();

        var averageScore = items.Count > 0 ? (decimal?)items.Average(e => e.Score) : null;

        return Ok(new EvaluationListResponse(items, items.Count, averageScore));
    }
}
