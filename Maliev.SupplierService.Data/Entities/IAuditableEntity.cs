namespace Maliev.SupplierService.Data.Entities;

/// <summary>
/// Defines basic audit properties for entities.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>Gets or sets the creation timestamp.</summary>
    DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the last update timestamp.</summary>
    DateTime UpdatedAt { get; set; }
}
