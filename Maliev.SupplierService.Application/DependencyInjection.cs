using Maliev.SupplierService.Application.Interfaces;
using Maliev.SupplierService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.SupplierService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISupplierService, Services.SupplierService>();
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }
}
