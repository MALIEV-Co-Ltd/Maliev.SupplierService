namespace Maliev.SupplierService.Api.Configuration;

/// <summary>
/// Defines the settings for external services that this service depends on.
/// </summary>
public class ExternalServicesSettings
{
    /// <summary>
    /// The name of the configuration section for external services.
    /// </summary>
    public const string SectionName = "ExternalServices";

    /// <summary>
    /// Gets or sets the endpoint configuration for the Purchase Order Service.
    /// </summary>
    public ServiceEndpoint PurchaseOrderService { get; set; } = new();

    /// <summary>
    /// Gets or sets the endpoint configuration for the Invoice Service.
    /// </summary>
    public ServiceEndpoint InvoiceService { get; set; } = new();

    /// <summary>
    /// Gets or sets the endpoint configuration for the Material Service.
    /// </summary>
    public ServiceEndpoint MaterialService { get; set; } = new();
}

/// <summary>
/// Represents the configuration for a single external service endpoint.
/// </summary>
public class ServiceEndpoint
{
    /// <summary>
    /// Gets or sets the base URL for the service endpoint.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the request timeout in seconds.
    /// </summary>
    public int TimeoutInSeconds { get; set; } = 30;
}
