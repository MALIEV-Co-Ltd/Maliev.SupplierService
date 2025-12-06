namespace Maliev.SupplierService.Api.DTOs.Responses;

/// <summary>
/// Represents a single audit log entry for a supplier-related change.
/// </summary>
/// <param name="Id">The unique identifier of the audit log entry.</param>
/// <param name="ChangeType">The type of change that occurred (e.g., Create, Update, Delete).</param>
/// <param name="EntityType">The type of entity that was affected (e.g., Supplier, Contact).</param>
/// <param name="EntityId">The unique identifier of the affected entity.</param>
/// <param name="OldValues">A JSON string representing the old values of the changed fields.</param>
/// <param name="NewValues">A JSON string representing the new values of the changed fields.</param>
/// <param name="ChangedBy">The ID of the user who made the change.</param>
/// <param name="ChangedByName">The name of the user who made the change.</param>
/// <param name="Timestamp">The timestamp when the change occurred.</param>
public record AuditLogResponse(
    Guid Id,
    string ChangeType,
    string EntityType,
    Guid EntityId,
    string? OldValues,
    string? NewValues,
    string ChangedBy,
    string ChangedByName,
    DateTime Timestamp
);

/// <summary>
/// Represents a paginated list of audit log entries.
/// </summary>
/// <param name="Items">A read-only list of audit log responses.</param>
/// <param name="TotalCount">The total number of audit log entries available.</param>
/// <param name="Page">The current page number (1-based).</param>
/// <param name="PageSize">The number of items per page.</param>
/// <param name="TotalPages">The total number of pages available.</param>
public record AuditLogListResponse(
    IReadOnlyList<AuditLogResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
