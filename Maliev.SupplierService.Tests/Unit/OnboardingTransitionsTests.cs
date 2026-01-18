using Maliev.SupplierService.Api.Services;
using Maliev.SupplierService.Data.Enums;
using Xunit;

namespace Maliev.SupplierService.Tests.Unit;

public class OnboardingTransitionsTests
{
    [Theory]
    [InlineData(OnboardingStage.PendingApproval, OnboardingStage.DocumentationReview, true)]
    [InlineData(OnboardingStage.DocumentationReview, OnboardingStage.FinalApproval, true)]
    [InlineData(OnboardingStage.DocumentationReview, OnboardingStage.PendingApproval, true)]
    [InlineData(OnboardingStage.Active, OnboardingStage.PendingApproval, false)]
    public void IsValidTransition_ValidatesCorrectly(OnboardingStage from, OnboardingStage to, bool expected)
    {
        // Act
        var result = OnboardingTransitions.IsValidTransition(from, to);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetValidNextStages_ReturnsExpectedStages()
    {
        // Act
        var nextStages = OnboardingTransitions.GetValidNextStages(OnboardingStage.PendingApproval);

        // Assert
        Assert.Single(nextStages);
        Assert.Equal(OnboardingStage.DocumentationReview, nextStages[0]);
    }
}
