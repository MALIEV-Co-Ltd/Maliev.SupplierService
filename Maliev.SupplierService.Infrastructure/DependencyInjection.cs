using Maliev.SupplierService.Application.Interfaces;
using Maliev.SupplierService.Infrastructure.Caching;
using Maliev.SupplierService.Infrastructure.ExternalServices;
using Maliev.SupplierService.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.SupplierService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISupplierDbContext>(provider => provider.GetRequiredService<SupplierDbContext>());

        services.AddScoped<ICacheService, RedisCacheService>();

        // Register HTTP Clients for external services
        services.AddHttpClient<IPurchaseOrderServiceClient, PurchaseOrderServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:PurchaseOrderService"] ?? "http://purchaseorder-service");
        });

        services.AddHttpClient<IInvoiceServiceClient, InvoiceServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:InvoiceService"] ?? "http://invoice-service");
        });

        services.AddHttpClient<IMaterialServiceClient, MaterialServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:MaterialService"] ?? "http://material-service");
        });

        return services;
    }
}
