using System.Text.Json;
using System.Text.Json.Serialization;
using Maliev.SupplierService.Data;
using Maliev.SupplierService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maliev.SupplierService.Api.Services;

/// <summary>
/// Provides services for logging audit trail entries related to supplier activities.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IDbContextFactory<SupplierDbContext> _contextFactory;
    private readonly ILogger<AuditService> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditService"/> class.
    /// </summary>
    /// <param name="contextFactory">The DbContext factory for creating <see cref="SupplierDbContext"/> instances.</param>
    /// <param name="logger">The logger instance.</param>
    public AuditService(IDbContextFactory<SupplierDbContext> contextFactory, ILogger<AuditService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Logs a change to an entity in the audit trail.
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
    public async Task LogChangeAsync(
        Guid supplierId,
        string changeType,
        string entityType,
        Guid entityId,
        object? oldValues,
        object? newValues,
        string userId,
        string userName,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var auditLog = new SupplierAuditLog
        {
            Id = Guid.NewGuid(),
            SupplierId = supplierId,
            ChangeType = changeType,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues is null ? null : JsonSerializer.Serialize(oldValues, SerializerOptions),
            NewValues = newValues is null ? null : JsonSerializer.Serialize(newValues, SerializerOptions),
            ChangedBy = userId,
            ChangedByName = userName,
            Timestamp = DateTime.UtcNow
        };

        context.SupplierAuditLogs.Add(auditLog);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Audit log created: {ChangeType} {EntityType} {EntityId} by {UserName}",
            changeType, entityType, entityId, userName);
    }
}
