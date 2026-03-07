using Maliev.SupplierService.Domain.Enums;
using Maliev.SupplierService.Domain.Validation;
using Xunit;

namespace Maliev.SupplierService.Tests.Unit;

public class OnboardingTransitionsTests
{
    [Theory]
    [InlineData(OnboardingStage.PendingApproval, OnboardingStage.Reviewing, true)]
    [InlineData(OnboardingStage.Reviewing, OnboardingStage.DocumentationReview, true)]
    [InlineData(OnboardingStage.DocumentationReview, OnboardingStage.FinalApproval, true)]
    [InlineData(OnboardingStage.FinalApproval, OnboardingStage.Approved, true)]
    [InlineData(OnboardingStage.Approved, OnboardingStage.Active, true)]
    [InlineData(OnboardingStage.Active, OnboardingStage.Rejected, true)]
    [InlineData(OnboardingStage.PendingApproval, OnboardingStage.Rejected, true)]
    [InlineData(OnboardingStage.PendingApproval, OnboardingStage.Approved, false)]
    [InlineData(OnboardingStage.Rejected, OnboardingStage.PendingApproval, true)]
    public void IsValidTransition_ValidatesCorrectly(OnboardingStage current, OnboardingStage next, bool expected)
    {
        // Act
        var result = OnboardingTransitions.IsValidTransition(current, next);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetValidNextStages_ReturnsExpectedStages()
    {
        // Act
        var stages = OnboardingTransitions.GetValidNextStages(OnboardingStage.PendingApproval).ToList();

        // Assert
        Assert.Contains(OnboardingStage.Reviewing, stages);
        Assert.Contains(OnboardingStage.Rejected, stages);
        Assert.Equal(2, stages.Count);
    }
}
