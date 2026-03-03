using Maliev.SupplierService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Maliev.SupplierService.Application.Interfaces;

public interface ISupplierDbContext
{
    DbSet<Supplier> Suppliers { get; }
    DbSet<SupplierContact> SupplierContacts { get; }
    DbSet<MaterialCategory> MaterialCategories { get; }
    DbSet<SupplierCapability> SupplierCapabilities { get; }
    DbSet<OnboardingStatus> OnboardingStatuses { get; }
    DbSet<SupplierCertification> SupplierCertifications { get; }
    DbSet<PerformanceEvaluation> PerformanceEvaluations { get; }
    DbSet<SupplierAuditLog> SupplierAuditLogs { get; }
    
    EntityEntry Entry(object entity);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
