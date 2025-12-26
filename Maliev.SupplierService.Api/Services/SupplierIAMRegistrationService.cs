using Maliev.Aspire.ServiceDefaults.IAM;
using Maliev.SupplierService.Api.Constants;
using Microsoft.Extensions.Logging;

namespace Maliev.SupplierService.Api.Services;

/// <summary>
/// Registers SupplierService permissions and roles with the central IAM system on startup.
/// </summary>
public class SupplierIAMRegistrationService : IAMRegistrationService
{
    private const string ServiceName = "supplier";

    /// <summary>
    /// Initializes a new instance of the <see cref="SupplierIAMRegistrationService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger.</param>
    public SupplierIAMRegistrationService(
        IHttpClientFactory httpClientFactory,
        ILogger<SupplierIAMRegistrationService> logger)
        : base(httpClientFactory, logger, ServiceName)
    {
    }

    /// <inheritdoc/>
    protected override IEnumerable<PermissionRegistration> GetPermissions()
    {
        return Permissions.GetAll().Select(p => new PermissionRegistration
        {
            PermissionId = p.Id,
            Description = p.Description
        });
    }

    /// <inheritdoc/>
    protected override IEnumerable<RoleRegistration> GetPredefinedRoles()
    {
        return Roles.GetDefinitions().Select(r => new RoleRegistration
        {
            RoleId = r.Name,
            Description = r.Description,
            PermissionIds = r.Permissions
        });
    }
}