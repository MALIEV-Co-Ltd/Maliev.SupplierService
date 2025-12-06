using Maliev.SupplierService.Api.DTOs.Requests;
using Maliev.SupplierService.Api.Events;
using Maliev.SupplierService.Data;
using Maliev.SupplierService.Data.Entities;
using Maliev.SupplierService.Data.Enums;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Maliev.SupplierService.Api.Services;

/// <summary>
/// Implements the core business logic and operations for managing suppliers.
/// </summary>
public class SupplierService : ISupplierService
{
    private readonly SupplierDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly IAuditService _auditService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<SupplierService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SupplierService"/> class.
    /// </summary>
    /// <param name="context">The database context for supplier data.</param>
    /// <param name="cacheService">The caching service.</param>
    /// <param name="auditService">The audit logging service.</param>
    /// <param name="publishEndpoint">The MassTransit publish endpoint for events.</param>
    /// <param name="logger">The logger instance.</param>
    public SupplierService(
        SupplierDbContext context,
        ICacheService cacheService,
        IAuditService auditService,
        IPublishEndpoint publishEndpoint,
        ILogger<SupplierService> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _auditService = auditService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new supplier asynchronously.
    /// </summary>
    /// <param name="companyName">The legal name of the supplier company.</param>
    /// <param name="taxId">The tax identification number of the supplier.</param>
    /// <param name="address">The street address of the supplier.</param>
    /// <param name="city">The city where the supplier is located.</param>
    /// <param name="country">The country where the supplier is located.</param>
    /// <param name="postalCode">The postal code for the supplier's address.</param>
    /// <param name="materialCategoryIds">A collection of IDs for the material categories the supplier provides.</param>
    /// <param name="capabilities">A list of the supplier's capabilities or services.</param>
    /// <param name="primaryContact">Optional primary contact for the supplier.</param>
    /// <param name="userId">The ID of the user creating the supplier.</param>
    /// <param name="userName">The name of the user creating the supplier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The newly created <see cref="Supplier"/> entity.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a supplier with the given TaxId already exists.</exception>
    public async Task<Supplier> CreateAsync(
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
        // Check for duplicate TaxId
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
            OnboardingStage = OnboardingStage.PendingApproval
        };

        // Add material categories
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

        // Add capabilities
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

        // Add primary contact if provided
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
                IsPrimary = true
            });
        }

        // Add initial onboarding status
        supplier.OnboardingHistory.Add(new OnboardingStatus
        {
            Id = Guid.NewGuid(),
            SupplierId = supplier.Id,
            Stage = OnboardingStage.PendingApproval,
            TransitionedBy = userId,
            TransitionedByName = userName,
            Notes = "Supplier created"
        });

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync(cancellationToken);

        // Log audit for supplier creation
        await _auditService.LogChangeAsync(
            supplier.Id,
            "CREATE",
            nameof(Supplier),
            supplier.Id,
            null,
            new { supplier.Id, supplier.CompanyName, supplier.TaxId },
            userId,
            userName,
            cancellationToken);

        // Invalidate cache
        await _cacheService.InvalidateByTagAsync("suppliers", cancellationToken);

        // Publish event
        await _publishEndpoint.Publish(new SupplierCreatedEvent(
            supplier.Id,
            supplier.CompanyName,
            supplier.TaxId,
            supplier.Country,
            supplier.CreatedAt,
            userId), cancellationToken);

        _logger.LogInformation("Created supplier {SupplierId} with TaxId {TaxId}", supplier.Id, taxId);

        return supplier;
    }

    /// <summary>
    /// Retrieves a supplier by its unique identifier asynchronously.
    /// </summary>
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

        // If setting as primary, unset other primary contacts
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
            IsPrimary = request.IsPrimary
        };

        supplier.Contacts.Add(contact);
        await _context.SaveChangesAsync(cancellationToken);

        // Log audit
        await _auditService.LogChangeAsync(
            supplierId,
            "ADD_CONTACT",
            nameof(SupplierContact),
            contact.Id,
            null,
            contact,
            userId,
            userName,
            cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveAsync($"supplier:{supplierId}", cancellationToken);

        _logger.LogInformation("Added contact {ContactId} to supplier {SupplierId}", contact.Id, supplierId);

        return contact;
    }

    /// <param name="id">The unique identifier of the supplier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The <see cref="Supplier"/> entity if found; otherwise, <c>null</c>.</returns>
    public async Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"supplier:{id}";
        var cached = await _cacheService.GetAsync<Supplier>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
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
            await _cacheService.SetAsync(cacheKey, supplier, cancellationToken: cancellationToken);
        }

        return supplier;
    }

    /// <summary>
    /// Validates if a supplier exists and is active asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the supplier to validate.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A tuple indicating whether the supplier is valid and the <see cref="Supplier"/> entity if found.</returns>
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

    /// <summary>
    /// Checks a supplier's eligibility for certain operations (e.g., participating in purchase orders) asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the supplier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A tuple indicating whether the supplier is eligible and a list of reasons for ineligibility.</returns>
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

        // Check if supplier is Active
        if (supplier.Status != SupplierStatus.Active)
        {
            reasons.Add($"Supplier status is {supplier.Status}, must be Active");
        }

        // Check for expired certifications
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

    /// <summary>
    /// Retrieves a read-only list of all available material categories asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A read-only list of <see cref="MaterialCategory"/> entities.</returns>
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

    /// <summary>
    /// Updates an existing supplier's information asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the supplier to update.</param>
    /// <param name="companyName">The updated legal name of the supplier company.</param>
    /// <param name="address">The updated street address of the supplier.</param>
    /// <param name="city">The updated city where the supplier is located.</param>
    /// <param name="country">The updated country where the supplier is located.</param>
    /// <param name="postalCode">The updated postal code for the supplier's address.</param>
    /// <param name="materialCategoryIds">An updated collection of IDs for the material categories the supplier provides.</param>
    /// <param name="capabilities">An updated list of the supplier's capabilities or services.</param>
    /// <param name="rowVersion">The row version for optimistic concurrency control.</param>
    /// <param name="userId">The ID of the user updating the supplier.</param>
    /// <param name="userName">The name of the user updating the supplier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The updated <see cref="Supplier"/> entity.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the supplier is not found.</exception>
    /// <exception cref="DbUpdateConcurrencyException">Thrown if a concurrency conflict occurs.</exception>
    public async Task<Supplier> UpdateAsync(
        Guid id,
        string? companyName,
        string? address,
        string? city,
        string? country,
        string? postalCode,
        IEnumerable<Guid>? materialCategoryIds,
        IEnumerable<string>? capabilities,
        long rowVersion,
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

        // Check optimistic concurrency using UpdatedAt ticks
        if (supplier.UpdatedAt.Ticks != rowVersion)
        {
            throw new DbUpdateConcurrencyException("The supplier has been modified by another user.");
        }

        var oldSupplier = new { supplier.CompanyName, supplier.Address, supplier.City, supplier.Country, supplier.PostalCode };
        var changedFields = new List<string>();

        // Update fields if provided
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
        if (postalCode != supplier.PostalCode)
        {
            supplier.PostalCode = postalCode;
            changedFields.Add("PostalCode");
        }

        // Update material categories if provided
        if (materialCategoryIds is not null)
        {
            supplier.MaterialCategories.Clear();
            var categories = await _context.MaterialCategories
                .Where(c => materialCategoryIds.Contains(c.Id) && c.IsActive)
                .ToListAsync(cancellationToken);
            foreach (var category in categories)
            {
                supplier.MaterialCategories.Add(category);
            }
            changedFields.Add("MaterialCategories");
        }

        // Update capabilities if provided
        if (capabilities is not null)
        {
            // Remove old capabilities
            _context.SupplierCapabilities.RemoveRange(supplier.Capabilities);
            supplier.Capabilities.Clear();

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
            changedFields.Add("Capabilities");
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Log audit
        await _auditService.LogChangeAsync(
            supplier.Id,
            "UPDATE",
            nameof(Supplier),
            supplier.Id,
            oldSupplier,
            supplier,
            userId,
            userName,
            cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveAsync($"supplier:{id}", cancellationToken);
        await _cacheService.InvalidateByTagAsync("suppliers", cancellationToken);

        // Publish event
        await _publishEndpoint.Publish(new SupplierUpdatedEvent(
            supplier.Id,
            supplier.CompanyName,
            changedFields,
            supplier.UpdatedAt,
            userId), cancellationToken);

        _logger.LogInformation("Updated supplier {SupplierId}", supplier.Id);

        return supplier;
    }

    /// <summary>
    /// Updates the status of an existing supplier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the supplier.</param>
    /// <param name="newStatus">The new status to apply to the supplier.</param>
    /// <param name="reason">An optional reason for the status change.</param>
    /// <param name="userId">The ID of the user updating the status.</param>
    /// <param name="userName">The name of the user updating the status.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The updated <see cref="Supplier"/> entity.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the supplier is not found.</exception>
    public async Task<Supplier> UpdateStatusAsync(
        Guid id,
        SupplierStatus newStatus,
        string? reason,
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

        var oldStatus = supplier.Status;
        supplier.Status = newStatus;

        await _context.SaveChangesAsync(cancellationToken);

        // Log audit
        await _auditService.LogChangeAsync(
            supplier.Id,
            "STATUS_CHANGE",
            nameof(Supplier),
            supplier.Id,
            new { Status = oldStatus, Reason = reason },
            new { Status = newStatus },
            userId,
            userName,
            cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveAsync($"supplier:{id}", cancellationToken);

        // Publish event
        await _publishEndpoint.Publish(new SupplierStatusChangedEvent(
            supplier.Id,
            oldStatus,
            newStatus,
            DateTime.UtcNow,
            userId), cancellationToken);

        _logger.LogInformation("Updated supplier {SupplierId} status from {OldStatus} to {NewStatus}", supplier.Id, oldStatus, newStatus);

        return supplier;
    }

    /// <summary>
    /// Updates specific metadata for a supplier asynchronously, typically used for external service callbacks.
    /// </summary>
    /// <param name="id">The unique identifier of the supplier.</param>
    /// <param name="lastOrderDate">The date of the supplier's last order.</param>
    /// <param name="totalOrderValue">The total value of all orders from the supplier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the supplier is not found.</exception>
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

        // Invalidate cache
        await _cacheService.RemoveAsync($"supplier:{id}", cancellationToken);

        _logger.LogDebug("Updated metadata for supplier {SupplierId}", supplier.Id);
    }

    /// <summary>
    /// Retrieves a paginated list of suppliers asynchronously based on various filtering and sorting criteria.
    /// </summary>
    /// <param name="page">The page number for pagination (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="status">An optional filter for the supplier's status.</param>
    /// <param name="categoryId">An optional filter for the material category ID.</param>
    /// <param name="capability">An optional filter for a specific supplier capability.</param>
    /// <param name="search">A search term to filter suppliers by name, tax ID, or other fields.</param>
    /// <param name="sortBy">The field to sort the results by.</param>
    /// <param name="sortOrder">The sort order ('asc' or 'desc').</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A tuple containing a read-only list of <see cref="Supplier"/> entities and the total count.</returns>
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

        // Apply filters
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
            var searchLower = search.ToLower();
            query = query.Where(s =>
                s.CompanyName.ToLower().Contains(searchLower) ||
                s.TaxId.ToLower().Contains(searchLower) ||
                s.City.ToLower().Contains(searchLower) ||
                s.Country.ToLower().Contains(searchLower));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
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

        // Apply pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>
    /// Adds a new certification to a supplier asynchronously.
    /// </summary>
    /// <param name="supplierId">The unique identifier of the supplier.</param>
    /// <param name="documentType">The type of certification document.</param>
    /// <param name="documentName">The name or title of the certification document.</param>
    /// <param name="issueDate">The date the certification was issued.</param>
    /// <param name="expirationDate">The optional expiration date of the certification.</param>
    /// <param name="externalFileRef">An optional external reference or URL to the certification file.</param>
    /// <param name="notes">Optional notes or comments about the certification.</param>
    /// <param name="userId">The ID of the user adding the certification.</param>
    /// <param name="userName">The name of the user adding the certification.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The newly created <see cref="SupplierCertification"/> entity.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the supplier is not found.</exception>
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
        await _context.SaveChangesAsync(cancellationToken);

        // Log audit
        await _auditService.LogChangeAsync(
            supplierId,
            "ADD_CERTIFICATION",
            nameof(SupplierCertification),
            certification.Id,
            null,
            certification,
            userId,
            userName,
            cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveAsync($"supplier:{supplierId}", cancellationToken);

        _logger.LogInformation("Added certification {CertificationId} to supplier {SupplierId}", certification.Id, supplierId);

        return certification;
    }

    /// <summary>
    /// Deletes a specific certification from a supplier asynchronously.
    /// </summary>
    /// <param name="supplierId">The unique identifier of the supplier.</param>
    /// <param name="certificationId">The unique identifier of the certification to delete.</param>
    /// <param name="userId">The ID of the user deleting the certification.</param>
    /// <param name="userName">The name of the user deleting the certification.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the certification or supplier is not found.</exception>
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
        await _context.SaveChangesAsync(cancellationToken);

        // Log audit
        await _auditService.LogChangeAsync(
            supplierId,
            "DELETE_CERTIFICATION",
            nameof(SupplierCertification),
            certificationId,
            certification,
            null,
            userId,
            userName,
            cancellationToken);

                // Invalidate cache

                await _cacheService.RemoveAsync($"supplier:{supplierId}", cancellationToken);

        

                _logger.LogInformation("Deleted certification {CertificationId} from supplier {SupplierId}", certificationId, supplierId);

            }

        

            /// <summary>
    /// Retrieves a read-only list of certifications that are expiring within a specified threshold asynchronously.
    /// </summary>
    /// <param name="daysThreshold">The number of days within which a certification is considered expiring soon.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A read-only list of tuples containing the <see cref="SupplierCertification"/>, its associated <see cref="Supplier"/>, and the remaining days until expiration.</returns>
    public async Task<IReadOnlyList<(SupplierCertification Certification, Supplier Supplier, int DaysUntilExpiration)>> GetExpiringCertificationsAsync(
        int daysThreshold,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var thresholdDate = today.AddDays(daysThreshold);

        var certifications = await _context.SupplierCertifications
            .Include(c => c.Supplier)
            .Where(c => c.ExpirationDate.HasValue &&
                        c.ExpirationDate.Value <= thresholdDate &&
                        c.ExpirationDate.Value >= today)
            .OrderBy(c => c.ExpirationDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return certifications
            .Select(c => (c, c.Supplier, c.ExpirationDate!.Value.DayNumber - today.DayNumber))
            .ToList();
    }

    /// <summary>
    /// Adds a new performance evaluation for a supplier asynchronously.
    /// </summary>
    /// <param name="supplierId">The unique identifier of the supplier.</param>
    /// <param name="category">The category of the performance evaluation.</param>
    /// <param name="score">The score given in the evaluation.</param>
    /// <param name="comments">Optional comments or feedback for the evaluation.</param>
    /// <param name="evaluationDate">The date the evaluation was conducted.</param>
    /// <param name="userId">The ID of the user adding the evaluation.</param>
    /// <param name="userName">The name of the user adding the evaluation.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The newly created <see cref="PerformanceEvaluation"/> entity.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the supplier is not found.</exception>
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
        await _context.SaveChangesAsync(cancellationToken);

        // Log audit
        await _auditService.LogChangeAsync(
            supplierId,
            "ADD_EVALUATION",
            nameof(PerformanceEvaluation),
            evaluation.Id,
            null,
            evaluation,
            userId,
            userName,
            cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveAsync($"supplier:{supplierId}", cancellationToken);

        _logger.LogInformation("Added evaluation {EvaluationId} to supplier {SupplierId}", evaluation.Id, supplierId);

        return evaluation;
    }

    /// <summary>
    /// Retrieves a read-only list of all performance evaluations for a specific supplier asynchronously.
    /// </summary>
    /// <param name="supplierId">The unique identifier of the supplier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A read-only list of <see cref="PerformanceEvaluation"/> entities.</returns>
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

    /// <summary>
    /// Advances the onboarding stage of a supplier asynchronously.
    /// </summary>
    /// <param name="supplierId">The unique identifier of the supplier.</param>
    /// <param name="targetStage">The target onboarding stage to transition the supplier to.</param>
    /// <param name="notes">Optional notes or comments related to the stage transition.</param>
    /// <param name="userId">The ID of the user advancing the onboarding stage.</param>
    /// <param name="userName">The name of the user advancing the onboarding stage.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The updated <see cref="Supplier"/> entity.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the supplier is not found or the transition is invalid.</exception>
    public async Task<Supplier> AdvanceOnboardingAsync(
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

        if (!OnboardingTransitions.IsValidTransition(supplier.OnboardingStage, targetStage))
        {
            var validStages = OnboardingTransitions.GetValidNextStages(supplier.OnboardingStage);
            throw new InvalidOperationException(
                $"Cannot transition from {supplier.OnboardingStage} to {targetStage}. Valid transitions: {string.Join(", ", validStages)}");
        }

        var oldStage = supplier.OnboardingStage;
        supplier.OnboardingStage = targetStage;

        // If reaching Active stage, also update supplier status
        if (targetStage == OnboardingStage.Active)
        {
            supplier.Status = SupplierStatus.Active;
        }

        // Record transition
        var onboardingStatus = new OnboardingStatus
        {
            Id = Guid.NewGuid(),
            SupplierId = supplierId,
            Stage = targetStage,
            TransitionedBy = userId,
            TransitionedByName = userName,
            Notes = notes
        };

        _context.OnboardingStatuses.Add(onboardingStatus);
        await _context.SaveChangesAsync(cancellationToken);

        // Log audit
        await _auditService.LogChangeAsync(
            supplierId,
            "ONBOARDING_TRANSITION",
            nameof(Supplier),
            supplierId,
            new { OnboardingStage = oldStage },
            new { OnboardingStage = targetStage },
            userId,
            userName,
            cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveAsync($"supplier:{supplierId}", cancellationToken);

        _logger.LogInformation("Advanced supplier {SupplierId} onboarding from {OldStage} to {NewStage}", supplierId, oldStage, targetStage);

        return supplier;
    }

    /// <summary>
    /// Retrieves the onboarding history for a specific supplier asynchronously.
    /// </summary>
    /// <param name="supplierId">The unique identifier of the supplier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A read-only list of <see cref="OnboardingStatus"/> entities representing the history.</returns>
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

    /// <summary>
    /// Retrieves the audit trail for a specific supplier asynchronously.
    /// </summary>
    /// <param name="supplierId">The unique identifier of the supplier.</param>
    /// <param name="startDate">Optional: The start date for filtering audit log entries.</param>
    /// <param name="endDate">Optional: The end date for filtering audit log entries.</param>
    /// <param name="page">The page number for pagination (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A tuple containing a read-only list of <see cref="SupplierAuditLog"/> entities and the total count.</returns>
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

    /// <summary>
    /// Deletes a supplier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the supplier to delete.</param>
    /// <param name="userId">The ID of the user deleting the supplier.</param>
    /// <param name="userName">The name of the user deleting the supplier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the supplier is not found.</exception>
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

        // Log audit before deletion
        await _auditService.LogChangeAsync(
            id,
            "DELETE",
            nameof(Supplier),
            id,
            supplier,
            null,
            userId,
            userName,
            cancellationToken);

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveAsync($"supplier:{id}", cancellationToken);
        await _cacheService.InvalidateByTagAsync("suppliers", cancellationToken);

        _logger.LogInformation("Deleted supplier {SupplierId}", id);
    }
}