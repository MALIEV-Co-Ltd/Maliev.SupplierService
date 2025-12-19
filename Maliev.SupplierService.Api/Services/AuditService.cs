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
    private readonly IServiceScopeFactory _scopeFactory;
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
    /// <param name="scopeFactory">The service scope factory for creating scoped DbContext instances.</param>
    /// <param name="logger">The logger instance.</param>
    public AuditService(IServiceScopeFactory scopeFactory, ILogger<AuditService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
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
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SupplierDbContext>();

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
