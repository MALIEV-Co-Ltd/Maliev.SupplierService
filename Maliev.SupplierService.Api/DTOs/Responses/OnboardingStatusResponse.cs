namespace Maliev.SupplierService.Api.DTOs.Responses;

using Maliev.SupplierService.Domain.Enums;

/// <summary>
/// Represents a single entry in a supplier's onboarding status history.
/// </summary>
/// <param name="Id">The unique identifier of the onboarding status entry.</param>
/// <param name="Stage">The onboarding stage the supplier transitioned to.</param>
/// <param name="TransitionedAt">The date and time when the transition occurred.</param>
/// <param name="TransitionedBy">The ID of the user who initiated the transition.</param>
/// <param name="TransitionedByName">The name of the user who initiated the transition.</param>
/// <param name="Notes">Optional notes or comments associated with the transition.</param>
public record OnboardingStatusResponse(
    Guid Id,
    OnboardingStage Stage,
    DateTime TransitionedAt,
    string TransitionedBy,
    string TransitionedByName,
    string? Notes
);

/// <summary>
/// Represents the full onboarding history for a supplier.
/// </summary>
/// <param name="CurrentStage">The supplier's current onboarding stage.</param>
/// <param name="History">A read-only list of historical onboarding status entries.</param>
public record OnboardingHistoryResponse(
    OnboardingStage CurrentStage,
    IReadOnlyList<OnboardingStatusResponse> History
);
