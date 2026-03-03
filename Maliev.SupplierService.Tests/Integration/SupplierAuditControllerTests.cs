using System.Net;
using System.Net.Http.Json;
using Maliev.SupplierService.Api.DTOs.Responses;
using Maliev.SupplierService.Domain.Entities;
using Maliev.SupplierService.Tests.Integration.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.SupplierService.Tests.Integration;

[Collection(nameof(IntegrationTestCollection))]
public class SupplierAuditControllerTests : BaseIntegrationTest
{
    public SupplierAuditControllerTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetAuditTrail_ReturnsAuditLogs()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync();

        using (var scope = Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<Maliev.SupplierService.Infrastructure.Persistence.SupplierDbContext>();
            dbContext.SupplierAuditLogs.Add(new SupplierAuditLog
            {
                Id = Guid.NewGuid(),
                SupplierId = supplier.Id,
                Action = "Created",
                EntityType = "Supplier",
                EntityId = supplier.Id,
                NewValues = "{\"CompanyName\": \"Test\"}",
                PerformedBy = Guid.NewGuid().ToString(),
                PerformedByName = "Admin",
                Timestamp = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await Client.GetAsync($"/supplier/v1/suppliers/{supplier.Id}/audit");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await GetResponseAsync<AuditLogListResponse>(response);
        Assert.NotNull(result);
        Assert.NotEmpty(result!.Items);
        Assert.Equal(1, result.TotalCount);
    }
}
