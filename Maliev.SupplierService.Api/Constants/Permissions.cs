namespace Maliev.SupplierService.Api.Constants;

/// <summary>
/// Defines granular permission identifiers for the SupplierService.
/// Format: service.resource.action
/// </summary>
public static class Permissions
{
    /// <summary>
    /// Service prefix for all permissions.
    /// </summary>
    public const string Prefix = "supplier";

    /// <summary>
    /// Supplier resource permissions.
    /// </summary>
    public static class Suppliers
    {
        /// <summary>Permission to create suppliers.</summary>
        public const string Create = "supplier.suppliers.create";
        /// <summary>Permission to read suppliers.</summary>
        public const string Read = "supplier.suppliers.read";
        /// <summary>Permission to update suppliers.</summary>
        public const string Update = "supplier.suppliers.update";
        /// <summary>Permission to delete suppliers.</summary>
        public const string Delete = "supplier.suppliers.delete";
        /// <summary>Permission to approve suppliers.</summary>
        public const string Approve = "supplier.suppliers.approve";
        /// <summary>Permission to suspend suppliers.</summary>
        public const string Suspend = "supplier.suppliers.suspend";
        /// <summary>Permission to export supplier data.</summary>
        public const string Export = "supplier.suppliers.export";
    }

    /// <summary>
    /// Contact resource permissions.
    /// </summary>
    public static class Contacts
    {
        /// <summary>Permission to create contacts.</summary>
        public const string Create = "supplier.contacts.create";
        /// <summary>Permission to read contacts.</summary>
        public const string Read = "supplier.contacts.read";
        /// <summary>Permission to update contacts.</summary>
        public const string Update = "supplier.contacts.update";
        /// <summary>Permission to delete contacts.</summary>
        public const string Delete = "supplier.contacts.delete";
    }

    /// <summary>
    /// Performance resource permissions.
    /// </summary>
    public static class Performance
    {
        /// <summary>Permission to view performance.</summary>
        public const string View = "supplier.performance.view";
        /// <summary>Permission to rate performance.</summary>
        public const string Rate = "supplier.performance.rate";
        /// <summary>Permission to generate performance reports.</summary>
        public const string Report = "supplier.performance.report";
    }

    /// <summary>
    /// Gets all registered permissions for this service.
    /// </summary>
    public static IEnumerable<(string Id, string Description)> GetAll()
    {
        return new[]
        {
            (Suppliers.Create, "Create new suppliers"),
            (Suppliers.Read, "Read supplier details"),
            (Suppliers.Update, "Update supplier information"),
            (Suppliers.Delete, "Delete suppliers"),
            (Suppliers.Approve, "Approve new suppliers"),
            (Suppliers.Suspend, "Suspend suppliers"),
            (Suppliers.Export, "Export supplier data"),

            (Contacts.Create, "Create supplier contacts"),
            (Contacts.Read, "Read contact details"),
            (Contacts.Update, "Update contacts"),
            (Contacts.Delete, "Delete contacts"),

            (Performance.View, "View supplier performance metrics"),
            (Performance.Rate, "Rate supplier performance"),
            (Performance.Report, "Generate performance reports")
        };
    }
}