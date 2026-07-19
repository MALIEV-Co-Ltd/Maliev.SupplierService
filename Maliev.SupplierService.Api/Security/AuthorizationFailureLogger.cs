using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Maliev.SupplierService.Api.Security;

/// <summary>
/// Custom result handler to log authorization failures as structured JSON.
/// </summary>
public class AuthorizationFailureLogger : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();
    private readonly ILogger<AuthorizationFailureLogger> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationFailureLogger"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public AuthorizationFailureLogger(ILogger<AuthorizationFailureLogger> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult result)
    {
        if (result.Forbidden || result.AuthorizationFailure != null)
        {
            var userId = context.User.FindFirst("sub")?.Value ?? "anonymous";
            var policyName = context.GetEndpoint()?.Metadata.GetMetadata<IAuthorizeData>()?.Policy ?? "Unknown";

            _logger.LogWarning(
                "Authorization failed for User: {UserId}. Policy: {PolicyName}. Reason: {Reason}",
                userId,
                policyName,
                result.Forbidden ? "Forbidden" : "Unauthorized");
        }

        await _defaultHandler.HandleAsync(next, context, policy, result);
    }
}
