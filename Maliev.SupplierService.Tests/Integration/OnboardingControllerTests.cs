using System.Net;
using System.Net.Http.Json;
using Maliev.SupplierService.Api.DTOs.Requests;
using Maliev.SupplierService.Api.DTOs.Responses;
using Maliev.SupplierService.Data.Enums;
using Maliev.SupplierService.Tests.Integration.Infrastructure;

namespace Maliev.SupplierService.Tests.Integration;

public class OnboardingControllerTests : BaseIntegrationTest
{
    public OnboardingControllerTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task AdvanceOnboarding_ValidTransition_Returns200()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync(); // Starts at PendingApproval

        var request = new AdvanceOnboardingRequest(
            TargetStage: OnboardingStage.DocumentationReview,
            Notes: "Documents received, starting review"
        );

        // Act
        var response = await Client.PostAsJsonAsync(
            $"/suppliers/v1/suppliers/{supplier.Id}/onboarding", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await GetResponseAsync<SupplierResponse>(response);
        Assert.Equal(OnboardingStage.DocumentationReview, result!.OnboardingStage);
    }

    [Fact]
    public async Task AdvanceOnboarding_InvalidTransition_Returns400()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync(); // Starts at PendingApproval

        // Try to skip directly to Active (invalid)
        var request = new AdvanceOnboardingRequest(
            TargetStage: OnboardingStage.Active,
            Notes: "Trying to skip stages"
        );

        // Act
        var response = await Client.PostAsJsonAsync(
            $"/suppliers/v1/suppliers/{supplier.Id}/onboarding", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AdvanceOnboarding_ToActive_SetsSupplierStatusActive()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync();

        // Progress through all stages
        await Client.PostAsJsonAsync(
            $"/suppliers/v1/suppliers/{supplier.Id}/onboarding",
            new AdvanceOnboardingRequest(OnboardingStage.DocumentationReview, "Step 1"));

        await Client.PostAsJsonAsync(
            $"/suppliers/v1/suppliers/{supplier.Id}/onboarding",
            new AdvanceOnboardingRequest(OnboardingStage.FinalApproval, "Step 2"));

        // Act - advance to Active
        var response = await Client.PostAsJsonAsync(
            $"/suppliers/v1/suppliers/{supplier.Id}/onboarding",
            new AdvanceOnboardingRequest(OnboardingStage.Active, "Approved!"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await GetResponseAsync<SupplierResponse>(response);
        Assert.Equal(OnboardingStage.Active, result!.OnboardingStage);
        Assert.Equal(SupplierStatus.Active, result.Status);
    }

    [Fact]
    public async Task GetOnboardingHistory_ReturnsAllTransitions()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync();

        // Make some transitions
        await Client.PostAsJsonAsync(
            $"/suppliers/v1/suppliers/{supplier.Id}/onboarding",
            new AdvanceOnboardingRequest(OnboardingStage.DocumentationReview, "First transition"));

        await Client.PostAsJsonAsync(
            $"/suppliers/v1/suppliers/{supplier.Id}/onboarding",
            new AdvanceOnboardingRequest(OnboardingStage.FinalApproval, "Second transition"));

        // Act
        var response = await Client.GetAsync(
            $"/suppliers/v1/suppliers/{supplier.Id}/onboarding");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await GetResponseAsync<OnboardingHistoryResponse>(response);
        Assert.NotNull(result);
        Assert.Equal(OnboardingStage.FinalApproval, result!.CurrentStage);
        Assert.True(result.History.Count >= 2);
    }

    [Fact]
    public async Task AdvanceOnboarding_NonExistingSupplier_Returns404()
    {
        // Arrange
        var request = new AdvanceOnboardingRequest(
            TargetStage: OnboardingStage.DocumentationReview,
            Notes: null
        );

        // Act
        var response = await Client.PostAsJsonAsync(
            $"/suppliers/v1/suppliers/{Guid.NewGuid()}/onboarding", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
