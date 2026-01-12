namespace Maliev.SupplierService.Api.Constants;

/// <summary>
/// Predefined roles for the Supplier Service.
/// Roles follow the GCP format: roles.supplier.{role-name}
/// </summary>
public static class SupplierPredefinedRoles
{
    /// <summary>Role for administrators with full access.</summary>
    public const string Admin = "roles.supplier.admin";
    /// <summary>Role for managers with broad access.</summary>
    public const string Manager = "roles.supplier.manager";
    /// <summary>Role for coordinators with operational access.</summary>
    public const string Coordinator = "roles.supplier.coordinator";
    /// <summary>Role for users with read-only access.</summary>
    public const string Viewer = "roles.supplier.viewer";

    /// <summary>
    /// Collection of all predefined roles for the Supplier Service.
    /// </summary>
    public static readonly IReadOnlyList<(string RoleId, string Description, string[] Permissions)> All = new List<(string, string, string[])>
    {
        (Admin, "Full access to all supplier-related operations", SupplierPermissions.All.ToArray()),

        (Manager, "Management access including approval and suspension", new[]
        {
            SupplierPermissions.Suppliers.Create,
            SupplierPermissions.Suppliers.Read,
            SupplierPermissions.Suppliers.Update,
            SupplierPermissions.Suppliers.Approve,
            SupplierPermissions.Suppliers.Suspend,
            SupplierPermissions.Contacts.Create,
            SupplierPermissions.Contacts.Read,
            SupplierPermissions.Contacts.Update,
            SupplierPermissions.Contacts.Delete,
            SupplierPermissions.Performance.View,
            SupplierPermissions.Performance.Rate,
            SupplierPermissions.Performance.Report
        }),

        (Coordinator, "Operational access to suppliers and contacts", new[]
        {
            SupplierPermissions.Suppliers.Create,
            SupplierPermissions.Suppliers.Read,
            SupplierPermissions.Suppliers.Update,
            SupplierPermissions.Contacts.Create,
            SupplierPermissions.Contacts.Read,
            SupplierPermissions.Contacts.Update,
            SupplierPermissions.Performance.View
        }),

        (Viewer, "Read-only access to supplier data", new[]
        {
            SupplierPermissions.Suppliers.Read,
            SupplierPermissions.Contacts.Read,
            SupplierPermissions.Performance.View
        })
    };
}
