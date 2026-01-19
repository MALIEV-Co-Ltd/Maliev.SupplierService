namespace Maliev.SupplierService.Api.Services.ExternalServices;

/// <summary>
/// Client for interacting with the external Purchase Order Service.
/// </summary>
public class PurchaseOrderServiceClient : IPurchaseOrderServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PurchaseOrderServiceClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PurchaseOrderServiceClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for making requests.</param>
    /// <param name="logger">The logger instance.</param>
    public PurchaseOrderServiceClient(HttpClient httpClient, ILogger<PurchaseOrderServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Checks if there are any existing purchase orders referencing the specified supplier in the Purchase Order Service.
    /// </summary>
    /// <param name="supplierId">The unique identifier of the supplier to check.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="DependencyCheckResult"/> indicating whether references exist and any associated messages.</returns>
    public async Task<DependencyCheckResult> CheckReferencesAsync(Guid supplierId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/suppliers/{supplierId}/references", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ReferenceCheckResponse>(cancellationToken: cancellationToken);
                return new DependencyCheckResult(
                    HasReferences: result?.ReferenceCount > 0,
                    ServiceName: "PurchaseOrderService",
                    ReferenceCount: result?.ReferenceCount ?? 0,
                    ErrorMessage: null,
                    ServiceUnavailable: false);
            }

            _logger.LogWarning("PurchaseOrderService returned {StatusCode} for supplier {SupplierId}",
                response.StatusCode, supplierId);

            return new DependencyCheckResult(
                HasReferences: false,
                ServiceName: "PurchaseOrderService",
                ReferenceCount: 0,
                ErrorMessage: $"Service returned {response.StatusCode}",
                ServiceUnavailable: true);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to check references with PurchaseOrderService for supplier {SupplierId}", supplierId);
            return new DependencyCheckResult(
                HasReferences: false,
                ServiceName: "PurchaseOrderService",
                ReferenceCount: 0,
                ErrorMessage: ex.Message,
                ServiceUnavailable: true);
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken != cancellationToken)
        {
            _logger.LogError(ex, "Timeout checking references with PurchaseOrderService for supplier {SupplierId}", supplierId);
            return new DependencyCheckResult(
                HasReferences: false,
                ServiceName: "PurchaseOrderService",
                ReferenceCount: 0,
                ErrorMessage: "Request timed out",
                ServiceUnavailable: true);
        }
    }

    private record ReferenceCheckResponse(int ReferenceCount);
}
