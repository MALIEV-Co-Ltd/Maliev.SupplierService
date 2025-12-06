using Maliev.SupplierService.Data.Enums;

namespace Maliev.SupplierService.Api.Services;

/// <summary>
/// Provides utility methods for managing valid transitions between supplier onboarding stages.
/// </summary>
public static class OnboardingTransitions
{
    private static readonly Dictionary<OnboardingStage, OnboardingStage[]> ValidTransitions = new()
    {
        [OnboardingStage.PendingApproval] = [OnboardingStage.DocumentationReview],
        [OnboardingStage.DocumentationReview] = [OnboardingStage.FinalApproval, OnboardingStage.PendingApproval],
        [OnboardingStage.FinalApproval] = [OnboardingStage.Active, OnboardingStage.DocumentationReview],
        [OnboardingStage.Active] = [] // Terminal state
    };

    /// <summary>
    /// Checks if a transition from one onboarding stage to another is valid.
    /// </summary>
    /// <param name="from">The current onboarding stage.</param>
    /// <param name="to">The target onboarding stage.</param>
    /// <returns><c>true</c> if the transition is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValidTransition(OnboardingStage from, OnboardingStage to)
    {
        return ValidTransitions.TryGetValue(from, out var validTargets) && validTargets.Contains(to);
    }

    /// <summary>
    /// Retrieves a read-only list of valid next stages for a given current onboarding stage.
    /// </summary>
    /// <param name="current">The current onboarding stage.</param>
    /// <returns>A read-only list of valid next <see cref="OnboardingStage"/> values.</returns>
    public static IReadOnlyList<OnboardingStage> GetValidNextStages(OnboardingStage current)
    {
        return ValidTransitions.TryGetValue(current, out var validTargets) ? validTargets : [];
    }
}
