using Maliev.SupplierService.Application.DTOs.Requests;
using Maliev.SupplierService.Application.Interfaces;
using Maliev.SupplierService.Domain.Entities;
using Maliev.SupplierService.Domain.Enums;
using Maliev.SupplierService.Domain.Validation;
using Maliev.MessagingContracts;
using Maliev.MessagingContracts.Contracts.Suppliers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Maliev.SupplierService.Application.Services;

/// <summary>
/// Implements the core business logic and operations for managing suppliers.
/// </summary>
public class SupplierService : ISupplierService
{
    private readonly ISupplierDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly IAuditService _auditService;
    private readonly IPurchaseOrderServiceClient _poClient;
    private readonly IInvoiceServiceClient _invoiceClient;
    private readonly IMaterialServiceClient _materialClient;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<SupplierService> _logger;

    public SupplierService(
        ISupplierDbContext context,
        ICacheService cacheService,
        IAuditService auditService,
        IPurchaseOrderServiceClient poClient,
        IInvoiceServiceClient invoiceClient,
        IMaterialServiceClient materialClient,
        IPublishEndpoint publishEndpoint,
        ILogger<SupplierService> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _auditService = auditService;
        _poClient = poClient;
        _invoiceClient = invoiceClient;
        _materialClient = materialClient;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<(Supplier Supplier, uint Xmin)> CreateAsync(
        string companyName,
        string taxId,
        string address,
        string city,
        string country,
        string? postalCode,
        IEnumerable<Guid>? materialCategoryIds,
        IEnumerable<string>? capabilities,
        CreateContactRequest? primaryContact,
        string userId,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var existingSupplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.TaxId == taxId, cancellationToken);

        if (existingSupplier is not null)
        {
            throw new InvalidOperationException("Supplier with this TaxId already exists.");
        }

        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            CompanyName = companyName,
            TaxId = taxId,
            Address = address,
            City = city,
            Country = country,
            PostalCode = postalCode,
            Status = SupplierStatus.PendingApproval,
            OnboardingStage = OnboardingStage.PendingApproval,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        if (materialCategoryIds?.Any() == true)
        {
            var categories = await _context.MaterialCategories
                .Where(c => materialCategoryIds.Contains(c.Id) && c.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var category in categories)
            {
                supplier.MaterialCategories.Add(category);
            }
        }

        if (capabilities?.Any() == true)
        {
            foreach (var capabilityName in capabilities)
            {
                supplier.Capabilities.Add(new SupplierCapability
                {
                    Id = Guid.NewGuid(),
                    SupplierId = supplier.Id,
                    Name = capabilityName,
                    IsActive = true
                });
            }
        }

        if (primaryContact is not null)
        {
            supplier.Contacts.Add(new SupplierContact
            {
                Id = Guid.NewGuid(),
                SupplierId = supplier.Id,
                Name = primaryContact.Name,
                Email = primaryContact.Email,
                Role = primaryContact.Role,
                Phone = primaryContact.Phone,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        supplier.OnboardingHistory.Add(new OnboardingStatus
        {
            Id = Guid.NewGuid(),
            SupplierId = supplier.Id,
            Stage = OnboardingStage.PendingApproval,
            TransitionedBy = userId,
            TransitionedByName = userName,
            TransitionedAt = DateTime.UtcNow,
            Notes = "Supplier created"
        });

        _context.Suppliers.Add(supplier);

        _auditService.LogChange(
            _context,
            supplier.Id,
            "CREATE",
            nameof(Supplier),
            supplier.Id,
            null,
            new { supplier.Id, supplier.CompanyName, supplier.TaxId },
            userId,
            userName);

        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"supplier:{supplier.Id}", cancellationToken);
        await _cacheService.RemoveByPatternAsync("supplier:list:*", cancellationToken);

        await _publishEndpoint.Publish(new SupplierCreatedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: "SupplierCreatedEvent",
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "SupplierService",
            ConsumedBy: ["PurchaseOrderService", "MaterialService", "NotificationService"],
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new SupplierCreatedEventPayload(
                SupplierId: supplier.Id,
                CompanyName: supplier.CompanyName,
                TaxId: supplier.TaxId,
                Country: supplier.Country,
                Status: supplier.Status.ToString(),
                CreatedBy: userId,
                CreatedAt: new DateTimeOffset(supplier.CreatedAt, TimeSpan.Zero)
            )
        ), cancellationToken);

        var xmin = GetXmin(supplier);
        return (supplier, xmin);
    }

    /// <inheritdoc/>
    public async Task<SupplierContact> AddContactAsync(
        Guid supplierId,
        CreateContactRequest request,
        string userId,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.Contacts)
            .FirstOrDefaultAsync(s => s.Id == supplierId, cancellationToken);

        if (supplier is null)
        {
            throw new InvalidOperationException("Supplier not found.");
        }

        if (request.IsPrimary)
        {
            foreach (var existingContact in supplier.Contacts.Where(c => c.IsPrimary))
            {
                existingContact.IsPrimary = false;
            }
        }

        var contact = new SupplierContact
        {
            Id = Guid.NewGuid(),
            SupplierId = supplierId,
            Name = request.Name,
            Email = request.Email,
            Role = request.Role,
            Phone = request.Phone,
            IsPrimary = request.IsPrimary,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.SupplierContacts.Add(contact);

        _auditService.LogChange(
            _context,
            supplierId,
            "ADD_CONTACT",
            nameof(SupplierContact),
            contact.Id,
            null,
            new { contact.Id, contact.Name, contact.Email, contact.Role, contact.Phone, contact.IsPrimary },
            userId,
            userName);

        await _context.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync($"supplier:{supplierId}", cancellationToken);

        return contact;
    }

    /// <inheritdoc/>
    public async Task<(Supplier? Supplier, uint Xmin)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"supplier:{id}";
        var xminCacheKey = $"supplier:{id}:xmin";

        var cached = await _cacheService.GetAsync<Supplier>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            var cachedXmin = await _cacheService.GetAsync<uint>(xminCacheKey, cancellationToken);
            return (cached, cachedXmin);
        }

        var supplier = await _context.Suppliers
            .Include(s => s.Contacts)
            .Include(s => s.MaterialCategories)
            .Include(s => s.Capabilities)
            .Include(s => s.Certifications)
            .Include(s => s.Evaluations)
            .Include(s => s.OnboardingHistory)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (supplier is not null)
        {
            var xmin = GetXmin(supplier);
            await _cacheService.SetAsync(cacheKey, supplier, TimeSpan.FromMinutes(15), cancellationToken);
            await _cacheService.SetAsync(xminCacheKey, xmin, TimeSpan.FromMinutes(15), cancellationToken);
            return (supplier, xmin);
        }

        return (null, 0u);
    }

    /// <inheritdoc/>
    public async Task<(bool IsValid, Supplier? Supplier)> ValidateSupplierAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var supplier = await _context.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (supplier is null)
        {
            return (false, null);
        }

        return (true, supplier);
    }

    /// <inheritdoc/>
    public async Task<(bool IsEligible, IReadOnlyList<string> Reasons)> CheckEligibilityAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.Certifications)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (supplier is null)
        {
            return (false, ["Supplier not found"]);
        }

        var reasons = new List<string>();

        if (supplier.Status != SupplierStatus.Active)
        {
            reasons.Add($"Supplier status is {supplier.Status}, must be Active");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expiredCerts = supplier.Certifications
            .Where(c => c.ExpirationDate.HasValue && c.ExpirationDate.Value < today)
            .ToList();

        if (expiredCerts.Count > 0)
        {
            reasons.Add($"Supplier has {expiredCerts.Count} expired certification(s)");
        }

        return (reasons.Count == 0, reasons);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MaterialCategory>> GetMaterialCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "material-categories";
        var cached = await _cacheService.GetAsync<List<MaterialCategory>>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var categories = await _context.MaterialCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        await _cacheService.SetAsync(cacheKey, categories, TimeSpan.FromMinutes(30), cancellationToken);

        return categories;
    }

    /// <inheritdoc/>
    public async Task<(Supplier Supplier, uint Xmin)> UpdateAsync(
        Guid id,
        string? companyName,
        string? address,
        string? city,
        string? country,
        string? postalCode,
        IEnumerable<Guid>? materialCategoryIds,
        IEnumerable<string>? capabilities,
        uint rowVersion,
        string userId,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.MaterialCategories)
            .Include(s => s.Capabilities)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (supplier is null)
        {
            throw new InvalidOperationException("Supplier not found.");
        }

        _context.Entry<Supplier>(supplier).Property<uint>("xmin").OriginalValue = rowVersion;

        var oldSupplier = new
        {
            supplier.CompanyName,
            supplier.Address,
            supplier.City,
            supplier.Country,
            supplier.PostalCode,
            MaterialCategoryIds = supplier.MaterialCategories.Select(c => c.Id).ToList(),
            Capabilities = supplier.Capabilities.Select(c => c.Name).ToList()
        };

        var changedFields = new List<string>();

        if (companyName is not null && companyName != supplier.CompanyName)
        {
            supplier.CompanyName = companyName;
            changedFields.Add("CompanyName");
        }
        if (address is not null && address != supplier.Address)
        {
            supplier.Address = address;
            changedFields.Add("Address");
        }
        if (city is not null && city != supplier.City)
        {
            supplier.City = city;
            changedFields.Add("City");
        }
        if (country is not null && country != supplier.Country)
        {
            supplier.Country = country;
            changedFields.Add("Country");
        }
        if (postalCode is not null && postalCode != supplier.PostalCode)
        {
            supplier.PostalCode = postalCode;
            changedFields.Add("PostalCode");
        }

        if (materialCategoryIds is not null)
        {
            var categoryIdsList = materialCategoryIds.Distinct().ToList();
            var categories = await _context.MaterialCategories
                .Where(c => categoryIdsList.Contains(c.Id) && c.IsActive)
                .ToListAsync(cancellationToken);

            if (categories.Count != categoryIdsList.Count)
            {
                throw new InvalidOperationException("One or more material category IDs are invalid or inactive.");
            }

            supplier.MaterialCategories.Clear();
            foreach (var category in categories)
            {
                supplier.MaterialCategories.Add(category);
            }
            changedFields.Add("MaterialCategories");
        }

        if (capabilities is not null)
        {
            var capabilityNames = capabilities.Distinct().ToList();
            var toRemove = supplier.Capabilities.Where(c => !capabilityNames.Contains(c.Name)).ToList();
            var toAdd = capabilityNames.Where(name => !supplier.Capabilities.Any(c => c.Name == name)).ToList();

            foreach (var capability in toRemove)
            {
                supplier.Capabilities.Remove(capability);
            }

            foreach (var name in toAdd)
            {
                supplier.Capabilities.Add(new SupplierCapability
                {
                    Id = Guid.NewGuid(),
                    SupplierId = supplier.Id,
                    Name = name,
                    IsActive = true
                });
            }
            changedFields.Add("Capabilities");
        }

        supplier.UpdatedAt = DateTime.UtcNow;
        supplier.UpdatedBy = userId;

        _auditService.LogChange(
            _context,
            supplier.Id,
            "UPDATE",
            nameof(Supplier),
            supplier.Id,
            oldSupplier,
            new
            {
                supplier.CompanyName,
                supplier.Address,
                supplier.City,
                supplier.Country,
                supplier.PostalCode,
                MaterialCategoryIds = supplier.MaterialCategories.Select(c => c.Id).ToList(),
                Capabilities = supplier.Capabilities.Select(c => c.Name).ToList()
            },
            userId,
            userName);

        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"supplier:{id}", cancellationToken);
        await _cacheService.RemoveByPatternAsync("supplier:list:*", cancellationToken);

        await _publishEndpoint.Publish(new SupplierUpdatedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: "SupplierUpdatedEvent",
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "SupplierService",
            ConsumedBy: ["PurchaseOrderService", "MaterialService", "NotificationService"],
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new SupplierUpdatedEventPayload(
                SupplierId: supplier.Id,
                CompanyName: supplier.CompanyName,
                ChangedFields: changedFields.ToArray(),
                UpdatedBy: userId,
                UpdatedAt: new DateTimeOffset(supplier.UpdatedAt, TimeSpan.Zero)
            )
        ), cancellationToken);

        var xmin = GetXmin(supplier);
        return (supplier, xmin);
    }

    /// <inheritdoc/>
    public async Task<(Supplier Supplier, uint Xmin)> UpdateStatusAsync(
        Guid id,
        SupplierStatus newStatus,
        string? reason,
        uint rowVersion,
        string userId,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (supplier is null)
        {
            throw new InvalidOperationException("Supplier not found.");
        }

        _context.Entry<Supplier>(supplier).Property<uint>("xmin").OriginalValue = rowVersion;

        var oldStatus = supplier.Status;
        supplier.Status = newStatus;
        supplier.UpdatedAt = DateTime.UtcNow;
        supplier.UpdatedBy = userId;

        _auditService.LogChange(
            _context,
            supplier.Id,
            "STATUS_CHANGE",
            nameof(Supplier),
            supplier.Id,
            new { Status = oldStatus, Reason = reason },
            new { Status = newStatus },
            userId,
            userName);

        await _context.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync($"supplier:{id}", cancellationToken);

        await _publishEndpoint.Publish(new SupplierStatusChangedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: "SupplierStatusChangedEvent",
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "SupplierService",
            ConsumedBy: ["PurchaseOrderService", "MaterialService", "NotificationService"],
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new SupplierStatusChangedEventPayload(
                SupplierId: supplier.Id,
                OldStatus: oldStatus.ToString(),
                NewStatus: newStatus.ToString(),
                ChangedBy: userId,
                ChangedAt: DateTimeOffset.UtcNow
            )
        ), cancellationToken);

        var xmin = GetXmin(supplier);
        return (supplier, xmin);
    }

    /// <inheritdoc/>
    public async Task UpdateMetadataAsync(
        Guid id,
        DateTime? lastOrderDate,
        decimal? totalOrderValue,
        CancellationToken cancellationToken = default)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (supplier is null)
        {
            throw new InvalidOperationException("Supplier not found.");
        }

        if (lastOrderDate.HasValue)
        {
            supplier.LastOrderDate = lastOrderDate;
        }
        if (totalOrderValue.HasValue)
        {
            supplier.TotalOrderValue = totalOrderValue;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync($"supplier:{id}", cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<Supplier> Items, int TotalCount)> ListSuppliersAsync(
        int page,
        int pageSize,
        SupplierStatus? status,
        Guid? categoryId,
        string? capability,
        string? search,
        string sortBy,
        string sortOrder,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Suppliers
            .Include(s => s.MaterialCategories)
            .Include(s => s.Capabilities)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(s => s.MaterialCategories.Any(c => c.Id == categoryId.Value));
        }

        if (!string.IsNullOrWhiteSpace(capability))
        {
            query = query.Where(s => s.Capabilities.Any(c => c.Name.Contains(capability) && c.IsActive));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s =>
                EF.Functions.ILike(s.CompanyName, $"%{search}%") ||
                EF.Functions.ILike(s.TaxId, $"%{search}%") ||
                EF.Functions.ILike(s.City, $"%{search}%") ||
                EF.Functions.ILike(s.Country, $"%{search}%"));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = sortBy.ToLower() switch
        {
            "companyname" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(s => s.CompanyName)
                : query.OrderBy(s => s.CompanyName),
            "createdat" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(s => s.CreatedAt)
                : query.OrderBy(s => s.CreatedAt),
            "updatedat" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(s => s.UpdatedAt)
                : query.OrderBy(s => s.UpdatedAt),
            "status" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(s => s.Status)
                : query.OrderBy(s => s.Status),
            "country" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(s => s.Country)
                : query.OrderBy(s => s.Country),
            _ => query.OrderBy(s => s.CompanyName)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<SupplierCertification> AddCertificationAsync(
        Guid supplierId,
        CertificationType documentType,
        string documentName,
        DateOnly issueDate,
        DateOnly? expirationDate,
        string? externalFileRef,
        string? notes,
        string userId,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == supplierId, cancellationToken);

        if (supplier is null)
        {
            throw new InvalidOperationException("Supplier not found.");
        }

        var certification = new SupplierCertification
        {
            Id = Guid.NewGuid(),
            SupplierId = supplierId,
            DocumentType = documentType,
            DocumentName = documentName,
            IssueDate = issueDate,
            ExpirationDate = expirationDate,
            ExternalFileRef = externalFileRef,
            Notes = notes
        };

        _context.SupplierCertifications.Add(certification);

        _auditService.LogChange(
            _context,
            supplierId,
            "ADD_CERTIFICATION",
            nameof(SupplierCertification),
            certification.Id,
            null,
            new { certification.Id, certification.DocumentType, certification.DocumentName, certification.IssueDate, certification.ExpirationDate },
            userId,
            userName);

        await _context.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync($"supplier:{supplierId}", cancellationToken);

        return certification;
    }

    /// <inheritdoc/>
    public async Task DeleteCertificationAsync(
        Guid supplierId,
        Guid certificationId,
        string userId,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var certification = await _context.SupplierCertifications
            .FirstOrDefaultAsync(c => c.Id == certificationId && c.SupplierId == supplierId, cancellationToken);

        if (certification is null)
        {
            throw new InvalidOperationException("Certification not found for supplier.");
        }

        _context.SupplierCertifications.Remove(certification);

        _auditService.LogChange(
            _context,
            supplierId,
            "DELETE_CERTIFICATION",
            nameof(SupplierCertification),
            certificationId,
            new { certification.Id, certification.DocumentType, certification.DocumentName },
            null,
            userId,
            userName);

        await _context.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync($"supplier:{supplierId}", cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<(SupplierCertification Certification, Supplier Supplier, int DaysUntilExpiration)> Items, int TotalCount)> GetExpiringCertificationsAsync(
        int daysThreshold,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var thresholdDate = today.AddDays(daysThreshold);

        var query = _context.SupplierCertifications
            .Include(c => c.Supplier)
            .Where(c => c.ExpirationDate.HasValue &&
                        c.ExpirationDate.Value <= thresholdDate &&
                        c.ExpirationDate.Value >= today);

        var totalCount = await query.CountAsync(cancellationToken);

        var certifications = await query
            .OrderBy(c => c.ExpirationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var items = certifications
            .Select(c => (c, c.Supplier, c.ExpirationDate!.Value.DayNumber - today.DayNumber))
            .ToList();

        return (items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<PerformanceEvaluation> AddEvaluationAsync(
        Guid supplierId,
        PerformanceRatingCategory category,
        int score,
        string? comments,
        DateOnly evaluationDate,
        string userId,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == supplierId, cancellationToken);

        if (supplier is null)
        {
            throw new InvalidOperationException("Supplier not found.");
        }

        var evaluation = new PerformanceEvaluation
        {
            Id = Guid.NewGuid(),
            SupplierId = supplierId,
            RatingCategory = category,
            Score = score,
            Notes = comments,
            EvaluationDate = evaluationDate,
            EvaluatorId = userId,
            EvaluatorName = userName
        };

        _context.PerformanceEvaluations.Add(evaluation);

        _auditService.LogChange(
            _context,
            supplierId,
            "ADD_EVALUATION",
            nameof(PerformanceEvaluation),
            evaluation.Id,
            null,
            new { evaluation.Id, evaluation.RatingCategory, evaluation.Score, evaluation.EvaluationDate },
            userId,
            userName);

        await _context.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync($"supplier:{supplierId}", cancellationToken);

        return evaluation;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PerformanceEvaluation>> GetEvaluationsAsync(
        Guid supplierId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PerformanceEvaluations
            .Where(e => e.SupplierId == supplierId)
            .OrderByDescending(e => e.EvaluationDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(Supplier Supplier, uint Xmin)> AdvanceOnboardingAsync(
        Guid supplierId,
        OnboardingStage targetStage,
        string? notes,
        string userId,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == supplierId, cancellationToken);

        if (supplier is null)
        {
            throw new InvalidOperationException("Supplier not found.");
        }

        if (supplier.Status != SupplierStatus.PendingApproval && supplier.OnboardingStage == OnboardingStage.Active)
        {
            throw new InvalidOperationException("Onboarding can only be managed for suppliers pending approval or in active onboarding flow.");
        }

        if (!OnboardingTransitions.IsValidTransition(supplier.OnboardingStage, targetStage))
        {
            var validStages = OnboardingTransitions.GetValidNextStages(supplier.OnboardingStage);
            throw new InvalidOperationException(
                $"Cannot transition from {supplier.OnboardingStage} to {targetStage}. Valid transitions: {string.Join(", ", validStages)}");
        }

        var oldStage = supplier.OnboardingStage;
        supplier.OnboardingStage = targetStage;

        if (targetStage == OnboardingStage.Active)
        {
            supplier.Status = SupplierStatus.Active;
        }
        else if (supplier.Status == SupplierStatus.Active)
        {
            supplier.Status = SupplierStatus.PendingApproval;
        }

        var onboardingStatus = new OnboardingStatus
        {
            Id = Guid.NewGuid(),
            SupplierId = supplierId,
            Stage = targetStage,
            TransitionedBy = userId,
            TransitionedByName = userName,
            TransitionedAt = DateTime.UtcNow,
            Notes = notes
        };

        _context.OnboardingStatuses.Add(onboardingStatus);

        _auditService.LogChange(
            _context,
            supplierId,
            "ONBOARDING_TRANSITION",
            nameof(Supplier),
            supplierId,
            new { OnboardingStage = oldStage },
            new { OnboardingStage = targetStage },
            userId,
            userName);

        await _context.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync($"supplier:{supplierId}", cancellationToken);

        var xmin = GetXmin(supplier);
        return (supplier, xmin);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<OnboardingStatus>> GetOnboardingHistoryAsync(
        Guid supplierId,
        CancellationToken cancellationToken = default)
    {
        return await _context.OnboardingStatuses
            .Where(o => o.SupplierId == supplierId)
            .OrderByDescending(o => o.TransitionedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<SupplierAuditLog> Items, int TotalCount)> GetAuditTrailAsync(
        Guid supplierId,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SupplierAuditLogs
            .Where(a => a.SupplierId == supplierId);

        if (startDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= endDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(
        Guid id,
        string userId,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (supplier is null)
        {
            throw new InvalidOperationException("Supplier not found.");
        }

        var poCheck = _poClient.CheckReferencesAsync(id, cancellationToken);
        var invoiceCheck = _invoiceClient.CheckReferencesAsync(id, cancellationToken);
        var materialCheck = _materialClient.CheckReferencesAsync(id, cancellationToken);

        await Task.WhenAll(poCheck, invoiceCheck, materialCheck);

        var dependencies = new List<string>();
        if (poCheck.Result.HasReferences) dependencies.Add("PurchaseOrderService");
        if (invoiceCheck.Result.HasReferences) dependencies.Add("InvoiceService");
        if (materialCheck.Result.HasReferences) dependencies.Add("MaterialService");

        if (dependencies.Count > 0)
        {
            throw new InvalidOperationException($"Supplier cannot be deleted as it is referenced by: {string.Join(", ", dependencies)}");
        }

        if (poCheck.Result.ServiceUnavailable || invoiceCheck.Result.ServiceUnavailable || materialCheck.Result.ServiceUnavailable)
        {
            throw new InvalidOperationException("Supplier deletion failed safely because one or more dependent services are unavailable.");
        }

        _auditService.LogChange(
            _context,
            id,
            "DELETE",
            nameof(Supplier),
            id,
            new { supplier.Id, supplier.CompanyName, supplier.TaxId },
            null,
            userId,
            userName);

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"supplier:{id}", cancellationToken);
        await _cacheService.RemoveByPatternAsync("supplier:list:*", cancellationToken);
    }

    /// <summary>Reads the xmin shadow property from the EF change tracker and returns its current value.</summary>
    private uint GetXmin(Supplier supplier)
    {
        return _context.Entry<Supplier>(supplier).Property<uint>("xmin").CurrentValue;
    }
}
