using Maliev.SupplierService.Api.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.SupplierService.Tests.Unit;

public class ServiceRegistrationTests
{
    [Fact]
    public void AddSupplierServices_RegistersExpectedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        services.AddSupplierServices(configuration);

        // Assert
        Assert.Contains(services, d => d.ServiceType == typeof(Maliev.SupplierService.Api.Services.ISupplierService));
        Assert.Contains(services, d => d.ServiceType == typeof(Maliev.SupplierService.Api.Services.IAuditService));
    }
}
