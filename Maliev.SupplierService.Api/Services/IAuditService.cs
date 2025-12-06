namespace Maliev.SupplierService.Api.Services;

/// <summary>
/// Defines the contract for an audit logging service specifically for supplier-related changes.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs a change to an entity in the audit trail asynchronously.
    /// </summary>
    /// <param name="supplierId">The ID of the supplier related to the change.</param>
    /// <param name="changeType">The type of change (e.g., "Created", "Updated", "Deleted").</param>
    /// <param name="entityType">The type of entity that was changed (e.g., "Supplier", "Contact").</param>
    /// <param name="entityId">The ID of the entity that was changed.</param>
    /// <param name="oldValues">Optional: The old values of the changed entity (serialized to JSON).</param>
    /// <param name="newValues">Optional: The new values of the changed entity (serialized to JSON).</param>
    /// <param name="userId">The ID of the user who performed the change.</param>
    /// <param name="userName">The name of the user who performed the change.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LogChangeAsync(
        Guid supplierId,
        string changeType,
        string entityType,
        Guid entityId,
        object? oldValues,
        object? newValues,
        string userId,
        string userName,
        CancellationToken cancellationToken = default);
}
