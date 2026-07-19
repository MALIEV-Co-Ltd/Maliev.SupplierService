using System.Net;
using Maliev.SupplierService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Maliev.SupplierService.Infrastructure.ExternalServices;

public class PurchaseOrderServiceClient : IPurchaseOrderServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PurchaseOrderServiceClient> _logger;

    public PurchaseOrderServiceClient(HttpClient httpClient, ILogger<PurchaseOrderServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<(bool HasReferences, bool ServiceUnavailable)> CheckReferencesAsync(Guid supplierId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/v1/purchase-orders/references/{supplierId}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return (content.Contains("true", StringComparison.OrdinalIgnoreCase), false);
            }
            return (false, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check PO references for supplier {SupplierId}", supplierId);
            return (false, true);
        }
    }
}
