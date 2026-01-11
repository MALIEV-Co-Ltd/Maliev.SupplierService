using Maliev.Aspire.ServiceDefaults.IAM;
using Maliev.SupplierService.Api.Constants;
using Microsoft.Extensions.Logging;

namespace Maliev.SupplierService.Api.Services;

/// <summary>
/// Registers SupplierService permissions and roles with the central IAM system on startup.
/// </summary>
public class SupplierIAMRegistrationService : IAMRegistrationService
{
    private const string ServiceNameValue = "supplier";

    /// <summary>
    /// Initializes a new instance of the <see cref="SupplierIAMRegistrationService"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="logger">The logger.</param>
    public SupplierIAMRegistrationService(
        IConfiguration configuration,
        ILogger<SupplierIAMRegistrationService> logger)
        : base(configuration, logger, ServiceNameValue)
    {
    }

    /// <inheritdoc/>
    protected override IEnumerable<PermissionRegistration> GetPermissions()
    {
        return SupplierPermissions.AllWithDescriptions.Select(p => new PermissionRegistration
        {
            PermissionId = p.Key,
            Description = p.Value
        });
    }

    /// <inheritdoc/>
    protected override IEnumerable<RoleRegistration> GetPredefinedRoles()
    {
        return SupplierPredefinedRoles.All.Select(r => new RoleRegistration
        {
            RoleId = r.RoleId,
            Description = r.Description,
            PermissionIds = r.Permissions.ToList(),
            IsCustom = false
        });
    }
}
