using Maliev.SupplierService.Domain.Validation;
using Maliev.SupplierService.Application;
using Maliev.SupplierService.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.SupplierService.Tests.Unit;
public class ServiceRegistrationTests
{
    [Fact]
    public void AddApplicationAndInfrastructure_RegistersExpectedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test",
                ["ExternalServices:PurchaseOrderService"] = "http://po",
                ["ExternalServices:InvoiceService"] = "http://invoice",
                ["ExternalServices:MaterialService"] = "http://material"
            })
            .Build();
        // Act
        services.AddApplication();
        services.AddInfrastructure(configuration);
        // Assert
        Assert.Contains(services, d => d.ServiceType == typeof(Maliev.SupplierService.Application.Interfaces.ISupplierService));
        Assert.Contains(services, d => d.ServiceType == typeof(Maliev.SupplierService.Application.Interfaces.IAuditService));
    }
}
