using System.Net;
using System.Net.Http.Json;
using Maliev.SupplierService.Api.Constants;
using Maliev.SupplierService.Api.DTOs.Requests;
using Maliev.SupplierService.Data.Enums;
using Maliev.SupplierService.Tests.Integration.Infrastructure;

namespace Maliev.SupplierService.Tests.Integration;

[Collection(nameof(IntegrationTestCollection))]
[Trait("Category", "Authorization")]
public class AuthorizationTests : BaseIntegrationTest
{
    public AuthorizationTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Admin_CanCreateSupplier()
    {
        // Arrange
        var adminClient = Factory.CreatePermissionAuthenticatedClient(
            permissions: Roles.GetDefinitions()
                .First(r => r.Name == Roles.Admin)
                .Permissions.ToArray());

        var request = new CreateSupplierRequest(
            CompanyName: "Admin Company",
            TaxId: "ADMIN123",
            Address: "Admin St",
            City: "Admin City",
            Country: "Admin Land",
            PostalCode: "12345",
            MaterialCategoryIds: null,
            Capabilities: null,
            PrimaryContact: null
        );

        // Act
        var response = await adminClient.PostAsJsonAsync("/supplier/v1/suppliers", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Viewer_CannotCreateSupplier()
    {
        // Arrange
        var viewerClient = Factory.CreatePermissionAuthenticatedClient(
            permissions: Roles.GetDefinitions()
                .First(r => r.Name == Roles.Viewer)
                .Permissions.ToArray());

        var request = new CreateSupplierRequest(
            CompanyName: "Viewer Company",
            TaxId: "VIEWER123",
            Address: "Viewer St",
            City: "Viewer City",
            Country: "Viewer Land",
            PostalCode: "12345",
            MaterialCategoryIds: null,
            Capabilities: null,
            PrimaryContact: null
        );

        // Act
        var response = await viewerClient.PostAsJsonAsync("/supplier/v1/suppliers", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Viewer_CanReadSuppliers()
    {
        // Arrange
        var viewerClient = Factory.CreatePermissionAuthenticatedClient(
            permissions: Roles.GetDefinitions()
                .First(r => r.Name == Roles.Viewer)
                .Permissions.ToArray());

        // Act
        var response = await viewerClient.GetAsync("/supplier/v1/suppliers");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Unauthorized_Returns403()
    {
        // Arrange
        var unauthorizedClient = Factory.CreatePermissionAuthenticatedClient(permissions: []);

        // Act
        var response = await unauthorizedClient.GetAsync("/supplier/v1/suppliers");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Coordinator_CanUpdateSupplier_ButCannotRatePerformance()
    {
        // Arrange
        var testUserId = Guid.NewGuid().ToString();
        var coordinatorPermissions = Roles.GetDefinitions()
            .First(r => r.Name == Roles.Coordinator)
            .Permissions.ToArray();
        var coordinatorClient = Factory.CreatePermissionAuthenticatedClient(
            userId: testUserId,
            permissions: coordinatorPermissions);

        var supplier = await CreateTestSupplierAsync();
        // Since CreateTestSupplierAsync uses a different scope/user in its implementation, 
        // let's ensure we have the fresh state and correct RowVersion
        var db = GetDbContext();
        var supplierFromDb = await db.Suppliers.FindAsync(supplier.Id);

        var updateRequest = new UpdateSupplierRequest(
            CompanyName: "Updated by Coordinator",
            Address: null,
            City: null,
            Country: null,
            PostalCode: null,
            MaterialCategoryIds: null,
            Capabilities: null,
            RowVersion: supplierFromDb!.UpdatedAt.Ticks.ToString()
        );

        var rateRequest = new CreateEvaluationRequest(
            Category: PerformanceRatingCategory.Quality,
            Score: 5,
            Comments: "Good",
            EvaluationDate: DateOnly.FromDateTime(DateTime.UtcNow)
        );

        // Act
        var updateResponse = await coordinatorClient.PutAsJsonAsync($"/supplier/v1/suppliers/{supplier.Id}", updateRequest);
        var rateResponse = await coordinatorClient.PostAsJsonAsync($"/supplier/v1/suppliers/{supplier.Id}/evaluations", rateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, rateResponse.StatusCode);
    }

    [Fact]
    public async Task Viewer_CannotUpdateSupplier()
    {
        // Arrange
        var viewerPermissions = Roles.GetDefinitions()
            .First(r => r.Name == Roles.Viewer)
            .Permissions.ToArray();
        var viewerClient = Factory.CreatePermissionAuthenticatedClient(permissions: viewerPermissions);

        var supplier = await CreateTestSupplierAsync();

        var updateRequest = new UpdateSupplierRequest(
            CompanyName: "Attempted Update",
            Address: null,
            City: null,
            Country: null,
            PostalCode: null,
            MaterialCategoryIds: null,
            Capabilities: null,
            RowVersion: supplier.UpdatedAt.Ticks.ToString()
        );

        // Act
        var response = await viewerClient.PutAsJsonAsync($"/supplier/v1/suppliers/{supplier.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
