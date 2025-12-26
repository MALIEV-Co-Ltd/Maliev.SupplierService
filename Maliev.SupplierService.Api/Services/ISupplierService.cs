using Maliev.SupplierService.Api.DTOs.Requests;
using Maliev.SupplierService.Data.Entities;
using Maliev.SupplierService.Data.Enums;

namespace Maliev.SupplierService.Api.Services;

/// <summary>
/// Defines the core business logic and operations for managing suppliers.
/// </summary>
public interface ISupplierService
{
    /// <summary>
    /// Creates a new supplier.
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
    Task<Supplier> CreateAsync(
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
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a supplier by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the supplier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The <see cref="Supplier"/> entity if found; otherwise, <c>null</c>.</returns>
    Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a contact person to an existing supplier.
    /// </summary>
    /// <param name="supplierId">The unique identifier of the supplier.</param>
    /// <param name="request">The contact details.</param>
    /// <param name="userId">The ID of the user adding the contact.</param>
    /// <param name="userName">The name of the user adding the contact.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The newly created <see cref="SupplierContact"/> entity.</returns>
    Task<SupplierContact> AddContactAsync(
        Guid supplierId,
        CreateContactRequest request,
        string userId,
        string userName,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Validates if a supplier exists and is active.
    /// </summary>
    /// <param name="id">The unique identifier of the supplier to validate.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A tuple indicating whether the supplier is valid and the <see cref="Supplier"/> entity if found.</returns>
    Task<(bool IsValid, Supplier? Supplier)> ValidateSupplierAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks a supplier's eligibility for certain operations (e.g., participating in purchase orders).
    /// </summary>
    /// <param name="id">The unique identifier of the supplier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A tuple indicating whether the supplier is eligible and a list of reasons for ineligibility.</returns>
    Task<(bool IsEligible, IReadOnlyList<string> Reasons)> CheckEligibilityAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a read-only list of all available material categories.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A read-only list of <see cref="MaterialCategory"/> entities.</returns>
    Task<IReadOnlyList<MaterialCategory>> GetMaterialCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing supplier's information.
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
    Task<Supplier> UpdateAsync(
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
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the status of an existing supplier.
    /// </summary>
    /// <param name="id">The unique identifier of the supplier.</param>
    /// <param name="newStatus">The new status to apply to the supplier.</param>
    /// <param name="reason">An optional reason for the status change.</param>
    /// <param name="userId">The ID of the user updating the status.</param>
    /// <param name="userName">The name of the user updating the status.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The updated <see cref="Supplier"/> entity.</returns>
    Task<Supplier> UpdateStatusAsync(
        Guid id,
        SupplierStatus newStatus,
        string? reason,
        string userId,
        string userName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates specific metadata for a supplier, typically used for external service callbacks.
    /// </summary>
    /// <param name="id">The unique identifier of the supplier.</param>
    /// <param name="lastOrderDate">The date of the supplier's last order.</param>
    /// <param name="totalOrderValue">The total value of all orders from the supplier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpdateMetadataAsync(
        Guid id,
        DateTime? lastOrderDate,
        decimal? totalOrderValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of suppliers based on various filtering and sorting criteria.
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
    Task<(IReadOnlyList<Supplier> Items, int TotalCount)> ListSuppliersAsync(
        int page,
        int pageSize,
        SupplierStatus? status,
        Guid? categoryId,
        string? capability,
        string? search,
        string sortBy,
        string sortOrder,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new certification to a supplier.
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
    Task<SupplierCertification> AddCertificationAsync(
        Guid supplierId,
        CertificationType documentType,
        string documentName,
        DateOnly issueDate,
        DateOnly? expirationDate,
        string? externalFileRef,
        string? notes,
        string userId,
        string userName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a specific certification from a supplier.
    /// </summary>
    /// <param name="supplierId">The unique identifier of the supplier.</param>
    /// <param name="certificationId">The unique identifier of the certification to delete.</param>
    /// <param name="userId">The ID of the user deleting the certification.</param>
    /// <param name="userName">The name of the user deleting the certification.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task DeleteCertificationAsync(
        Guid supplierId,
        Guid certificationId,
        string userId,
        string userName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a read-only list of certifications that are expiring within a specified threshold.
    /// </summary>
    /// <param name="daysThreshold">The number of days within which a certification is considered expiring soon.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A read-only list of tuples containing the <see cref="SupplierCertification"/>, its associated <see cref="Supplier"/>, and the remaining days until expiration.</returns>
    Task<IReadOnlyList<(SupplierCertification Certification, Supplier Supplier, int DaysUntilExpiration)>> GetExpiringCertificationsAsync(
        int daysThreshold,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new performance evaluation for a supplier.
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
    Task<PerformanceEvaluation> AddEvaluationAsync(
        Guid supplierId,
        PerformanceRatingCategory category,
        int score,
        string? comments,
        DateOnly evaluationDate,
        string userId,
        string userName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a read-only list of all performance evaluations for a specific supplier.
    /// </summary>
    /// <param name="supplierId">The unique identifier of the supplier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A read-only list of <see cref="PerformanceEvaluation"/> entities.</returns>
    Task<IReadOnlyList<PerformanceEvaluation>> GetEvaluationsAsync(
        Guid supplierId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Advances the onboarding stage of a supplier.
    /// </summary>
    /// <param name="supplierId">The unique identifier of the supplier.</param>
    /// <param name="targetStage">The target onboarding stage to transition the supplier to.</param>
    /// <param name="notes">Optional notes or comments related to the stage transition.</param>
    /// <param name="userId">The ID of the user advancing the onboarding stage.</param>
    /// <param name="userName">The name of the user advancing the onboarding stage.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The updated <see cref="Supplier"/> entity.</returns>
    Task<Supplier> AdvanceOnboardingAsync(
        Guid supplierId,
        OnboardingStage targetStage,
        string? notes,
        string userId,
        string userName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the onboarding history for a specific supplier.
    /// </summary>
    /// <param name="supplierId">The unique identifier of the supplier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A read-only list of <see cref="OnboardingStatus"/> entities representing the history.</returns>
    Task<IReadOnlyList<OnboardingStatus>> GetOnboardingHistoryAsync(
        Guid supplierId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the audit trail for a specific supplier.
    /// </summary>
    /// <param name="supplierId">The unique identifier of the supplier.</param>
    /// <param name="startDate">Optional: The start date for filtering audit log entries.</param>
    /// <param name="endDate">Optional: The end date for filtering audit log entries.</param>
    /// <param name="page">The page number for pagination (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A tuple containing a read-only list of <see cref="SupplierAuditLog"/> entities and the total count.</returns>
    Task<(IReadOnlyList<SupplierAuditLog> Items, int TotalCount)> GetAuditTrailAsync(
        Guid supplierId,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a supplier.
    /// </summary>
    /// <param name="id">The unique identifier of the supplier to delete.</param>
    /// <param name="userId">The ID of the user deleting the supplier.</param>
    /// <param name="userName">The name of the user deleting the supplier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task DeleteAsync(
        Guid id,
        string userId,
        string userName,
        CancellationToken cancellationToken = default);
}
