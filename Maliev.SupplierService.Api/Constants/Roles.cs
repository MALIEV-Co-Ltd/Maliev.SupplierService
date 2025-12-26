namespace Maliev.SupplierService.Api.Constants;

/// <summary>
/// Defines predefined roles and their associated permissions for the SupplierService.
/// </summary>
public static class Roles
{
    /// <summary>Admin role name.</summary>
    public const string Admin = "supplier-admin";
    /// <summary>Manager role name.</summary>
    public const string Manager = "supplier-manager";
    /// <summary>Coordinator role name.</summary>
    public const string Coordinator = "supplier-coordinator";
    /// <summary>Viewer role name.</summary>
    public const string Viewer = "supplier-viewer";

    /// <summary>
    /// Gets all predefined roles and their mapped permission identifiers.
    /// </summary>
    public static IEnumerable<(string Name, string Description, List<string> Permissions)> GetDefinitions()
    {
        var allPermissions = Permissions.GetAll().Select(p => p.Id).ToList();

        return new List<(string Name, string Description, List<string> Permissions)>
        {
            (
                Admin,
                "Full access to all supplier-related operations",
                allPermissions
            ),
            (
                Manager,
                "Management access including approval and suspension",
                new List<string>
                {
                    Permissions.Suppliers.Create,
                    Permissions.Suppliers.Read,
                    Permissions.Suppliers.Update,
                    Permissions.Suppliers.Approve,
                    Permissions.Suppliers.Suspend,
                    Permissions.Contacts.Create,
                    Permissions.Contacts.Read,
                    Permissions.Contacts.Update,
                    Permissions.Contacts.Delete,
                    Permissions.Performance.View,
                    Permissions.Performance.Rate,
                    Permissions.Performance.Report
                }
            ),
            (
                Coordinator,
                "Operational access to suppliers and contacts",
                new List<string>
                {
                    Permissions.Suppliers.Create,
                    Permissions.Suppliers.Read,
                    Permissions.Suppliers.Update,
                    Permissions.Contacts.Create,
                    Permissions.Contacts.Read,
                    Permissions.Contacts.Update,
                    Permissions.Performance.View
                }
            ),
            (
                Viewer,
                "Read-only access to supplier data",
                new List<string>
                {
                    Permissions.Suppliers.Read,
                    Permissions.Contacts.Read,
                    Permissions.Performance.View
                }
            )
        };
    }
}