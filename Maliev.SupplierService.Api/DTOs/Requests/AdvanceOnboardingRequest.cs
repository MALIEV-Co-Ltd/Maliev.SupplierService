namespace Maliev.SupplierService.Api.DTOs.Requests;

using Maliev.SupplierService.Domain.Enums;

/// <summary>
/// Represents a request to advance a supplier's onboarding process to a new stage.
/// </summary>
/// <param name="TargetStage">The target stage to transition the supplier to.</param>
/// <param name="Notes">Optional notes or comments related to the stage transition.</param>
public record AdvanceOnboardingRequest(
    OnboardingStage TargetStage,
    string? Notes
);
