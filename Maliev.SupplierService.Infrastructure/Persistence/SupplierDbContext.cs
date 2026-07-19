using Maliev.SupplierService.Application.Interfaces;
using Maliev.SupplierService.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Maliev.SupplierService.Infrastructure.Persistence;

public class SupplierDbContext : DbContext, ISupplierDbContext
{
    public SupplierDbContext(DbContextOptions<SupplierDbContext> options) : base(options)
    {
    }

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierContact> SupplierContacts => Set<SupplierContact>();
    public DbSet<MaterialCategory> MaterialCategories => Set<MaterialCategory>();
    public DbSet<SupplierCapability> SupplierCapabilities => Set<SupplierCapability>();
    public DbSet<OnboardingStatus> OnboardingStatuses => Set<OnboardingStatus>();
    public DbSet<SupplierCertification> SupplierCertifications => Set<SupplierCertification>();
    public DbSet<PerformanceEvaluation> PerformanceEvaluations => Set<PerformanceEvaluation>();
    public DbSet<SupplierAuditLog> SupplierAuditLogs => Set<SupplierAuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        // Configure xmin for optimistic concurrency (PostgreSQL system column)
        modelBuilder.Entity<Supplier>()
            .Property<uint>("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        // Configure relationships
        modelBuilder.Entity<Supplier>()
            .HasMany(s => s.Contacts)
            .WithOne(c => c.Supplier)
            .HasForeignKey(c => c.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Supplier>()
            .HasMany(s => s.MaterialCategories)
            .WithMany(c => c.Suppliers)
            .UsingEntity(j => j.ToTable("SupplierMaterialCategories"));

        modelBuilder.Entity<Supplier>()
            .HasMany(s => s.Capabilities)
            .WithOne(c => c.Supplier)
            .HasForeignKey(c => c.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Supplier>()
            .HasMany(s => s.OnboardingHistory)
            .WithOne(o => o.Supplier)
            .HasForeignKey(o => o.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Supplier>()
            .HasMany(s => s.Certifications)
            .WithOne(c => c.Supplier)
            .HasForeignKey(c => c.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Supplier>()
            .HasMany(s => s.Evaluations)
            .WithOne(e => e.Supplier)
            .HasForeignKey(e => e.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SupplierAuditLog>()
            .HasOne(a => a.Supplier)
            .WithMany()
            .HasForeignKey(a => a.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        // Table names
        modelBuilder.Entity<Supplier>().ToTable("Suppliers");
        modelBuilder.Entity<SupplierContact>().ToTable("SupplierContacts");
        modelBuilder.Entity<MaterialCategory>().ToTable("MaterialCategories");
        modelBuilder.Entity<SupplierCapability>().ToTable("SupplierCapabilities");
        modelBuilder.Entity<OnboardingStatus>().ToTable("OnboardingStatuses");
        modelBuilder.Entity<SupplierCertification>().ToTable("SupplierCertifications");
        modelBuilder.Entity<PerformanceEvaluation>().ToTable("PerformanceEvaluations");
        modelBuilder.Entity<SupplierAuditLog>().ToTable("SupplierAuditLogs");
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}
