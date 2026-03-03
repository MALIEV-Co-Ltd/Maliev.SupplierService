using System.Text.Json;
using System.Text.Json.Serialization;
using Maliev.SupplierService.Application.Interfaces;
using Maliev.SupplierService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Maliev.SupplierService.Application.Services;

/// <summary>
/// Provides services for logging audit trail entries related to supplier activities.
/// </summary>
public class AuditService : IAuditService
{
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
    /// <param name="logger">The logger instance.</param>
    public AuditService(ILogger<AuditService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public void LogChange(
        ISupplierDbContext context,
        Guid supplierId,
        string action,
        string entityType,
        Guid entityId,
        object? oldValues,
        object? newValues,
        string performedBy,
        string performedByName)
    {
        var auditLog = new SupplierAuditLog
        {
            Id = Guid.NewGuid(),
            SupplierId = supplierId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues is null ? null : JsonSerializer.Serialize(oldValues, SerializerOptions),
            NewValues = newValues is null ? null : JsonSerializer.Serialize(newValues, SerializerOptions),
            PerformedBy = performedBy,
            PerformedByName = performedByName,
            Timestamp = DateTime.UtcNow
        };

        context.SupplierAuditLogs.Add(auditLog);

        _logger.LogInformation(
            "Audit log added to change tracker: {Action} {EntityType} {EntityId} by {PerformedByName}",
            action, entityType, entityId, performedByName);
    }
}
