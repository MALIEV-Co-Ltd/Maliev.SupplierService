using System.Net;
using System.Net.Http.Json;
using Maliev.SupplierService.Api.DTOs.Requests;
using Maliev.SupplierService.Api.DTOs.Responses;
using Maliev.SupplierService.Domain.Enums;
using Maliev.SupplierService.Tests.Integration.Infrastructure;
using Xunit;

namespace Maliev.SupplierService.Tests.Integration;

[Collection(nameof(IntegrationTestCollection))]
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
        var (supplier, _) = await CreateTestSupplierAsync();
        var request = new AdvanceOnboardingRequest(
            TargetStage: OnboardingStage.Reviewing,
            Notes: "Moving to review stage"
        );

        // Act
        var response = await Client.PostAsJsonAsync($"/supplier/v1/suppliers/{supplier.Id}/onboarding", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await GetResponseAsync<SupplierResponse>(response);
        Assert.Equal(OnboardingStage.Reviewing, updated!.OnboardingStage);
    }

    [Fact]
    public async Task GetOnboardingHistory_ReturnsHistory()
    {
        // Arrange
        var (supplier, _) = await CreateTestSupplierAsync();

        // Advance stage to create history
        var request = new AdvanceOnboardingRequest(
            TargetStage: OnboardingStage.Reviewing,
            Notes: "Review started"
        );
        await Client.PostAsJsonAsync($"/supplier/v1/suppliers/{supplier.Id}/onboarding", request);

        // Act
        var response = await Client.GetAsync($"/supplier/v1/suppliers/{supplier.Id}/onboarding");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await GetResponseAsync<OnboardingHistoryResponse>(response);
        Assert.NotNull(result);
        Assert.Equal(OnboardingStage.Reviewing, result!.CurrentStage);
        Assert.NotEmpty(result.History);
    }
}
