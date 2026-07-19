using Maliev.SupplierService.Domain.Enums;

namespace Maliev.SupplierService.Domain.Entities;

public class Supplier
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public SupplierStatus Status { get; set; }
    public OnboardingStage OnboardingStage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? LastOrderDate { get; set; }
    public decimal? TotalOrderValue { get; set; }

    public ICollection<SupplierContact> Contacts { get; set; } = new List<SupplierContact>();
    public ICollection<MaterialCategory> MaterialCategories { get; set; } = new List<MaterialCategory>();
    public ICollection<SupplierCapability> Capabilities { get; set; } = new List<SupplierCapability>();
    public ICollection<OnboardingStatus> OnboardingHistory { get; set; } = new List<OnboardingStatus>();
    public ICollection<SupplierCertification> Certifications { get; set; } = new List<SupplierCertification>();
    public ICollection<PerformanceEvaluation> Evaluations { get; set; } = new List<PerformanceEvaluation>();
}
