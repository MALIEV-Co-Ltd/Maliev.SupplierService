using Maliev.SupplierService.Domain.Enums;

namespace Maliev.SupplierService.Domain.Entities;

public class OnboardingStatus
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public OnboardingStage Stage { get; set; }
    public string TransitionedBy { get; set; } = string.Empty;
    public string TransitionedByName { get; set; } = string.Empty;
    public DateTime TransitionedAt { get; set; }
    public string? Notes { get; set; }
    
    public Supplier Supplier { get; set; } = null!;
}
