using Maliev.SupplierService.Domain.Enums;

namespace Maliev.SupplierService.Domain.Validation;

public static class OnboardingTransitions
{
    public static bool IsValidTransition(OnboardingStage current, OnboardingStage next)
    {
        return current switch
        {
            OnboardingStage.PendingApproval => next is OnboardingStage.Reviewing or OnboardingStage.Rejected,
            OnboardingStage.Reviewing => next is OnboardingStage.DocumentationReview or OnboardingStage.Rejected,
            OnboardingStage.DocumentationReview => next is OnboardingStage.FinalApproval or OnboardingStage.Rejected,
            OnboardingStage.FinalApproval => next is OnboardingStage.Approved or OnboardingStage.Rejected,
            OnboardingStage.Approved => next is OnboardingStage.Active,
            OnboardingStage.Active => next is OnboardingStage.Rejected, // Can be revoked
            OnboardingStage.Rejected => next is OnboardingStage.PendingApproval, // Can re-apply
            _ => false
        };
    }

    public static IEnumerable<OnboardingStage> GetValidNextStages(OnboardingStage current)
    {
        return current switch
        {
            OnboardingStage.PendingApproval => [OnboardingStage.Reviewing, OnboardingStage.Rejected],
            OnboardingStage.Reviewing => [OnboardingStage.DocumentationReview, OnboardingStage.Rejected],
            OnboardingStage.DocumentationReview => [OnboardingStage.FinalApproval, OnboardingStage.Rejected],
            OnboardingStage.FinalApproval => [OnboardingStage.Approved, OnboardingStage.Rejected],
            OnboardingStage.Approved => [OnboardingStage.Active],
            OnboardingStage.Active => [OnboardingStage.Rejected],
            OnboardingStage.Rejected => [OnboardingStage.PendingApproval],
            _ => []
        };
    }
}
