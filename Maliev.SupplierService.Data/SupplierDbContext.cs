using Maliev.SupplierService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Maliev.Aspire.ServiceDefaults.Database;
using MassTransit;

namespace Maliev.SupplierService.Data;

public class SupplierDbContext : DbContext
{
    public SupplierDbContext(DbContextOptions<SupplierDbContext> options)
        : base(options)
    {
    }

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierContact> SupplierContacts => Set<SupplierContact>();
    public DbSet<MaterialCategory> MaterialCategories => Set<MaterialCategory>();
    public DbSet<SupplierCapability> SupplierCapabilities => Set<SupplierCapability>();
    public DbSet<SupplierCertification> SupplierCertifications => Set<SupplierCertification>();
    public DbSet<PerformanceEvaluation> PerformanceEvaluations => Set<PerformanceEvaluation>();
    public DbSet<SupplierAuditLog> SupplierAuditLogs => Set<SupplierAuditLog>();
    public DbSet<OnboardingStatus> OnboardingStatuses => Set<OnboardingStatus>();

    // MassTransit Outbox Entities
    public DbSet<MassTransit.EntityFrameworkCoreIntegration.OutboxMessage> OutboxMessages => Set<MassTransit.EntityFrameworkCoreIntegration.OutboxMessage>();
    public DbSet<MassTransit.EntityFrameworkCoreIntegration.OutboxState> OutboxStates => Set<MassTransit.EntityFrameworkCoreIntegration.OutboxState>();
    public DbSet<MassTransit.EntityFrameworkCoreIntegration.InboxState> InboxStates => Set<MassTransit.EntityFrameworkCoreIntegration.InboxState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SupplierDbContext).Assembly);

        // Configure MassTransit Outbox
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        // Configure UpdatedAt as a concurrency token for all auditable entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IAuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(IAuditableEntity.UpdatedAt))
                    .IsConcurrencyToken();
            }
        }

        // Apply PostgreSQL snake_case naming convention globally
        SnakeCaseNamingHelper.ApplySnakeCaseNaming(modelBuilder);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                if (entry.Entity is IAuditableEntity auditable)
                {
                    if (entry.State == EntityState.Added)
                    {
                        auditable.CreatedAt = now;
                    }
                    auditable.UpdatedAt = now;

                    if (entry.Entity is Supplier supplier)
                    {
                        supplier.RowVersion = Guid.NewGuid().ToByteArray();
                    }
                }
                else if (entry.State == EntityState.Added)
                {
                    if (entry.Entity is PerformanceEvaluation evaluation)
                    {
                        evaluation.CreatedAt = now;
                    }
                    else if (entry.Entity is SupplierAuditLog auditLog)
                    {
                        auditLog.Timestamp = now;
                    }
                    else if (entry.Entity is OnboardingStatus onboarding)
                    {
                        onboarding.TransitionedAt = now;
                    }
                }
            }
        }
    }
}
