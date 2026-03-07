using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Maliev.SupplierService.Tests.Integration.Infrastructure;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var permissionKey = "";

        // Extract permission key from authentication scheme (format: "Test-key")
        if (Context.Request.Headers.Authorization.Count > 0)
        {
            var authHeader = Context.Request.Headers.Authorization[0] ?? "";
            var parts = authHeader.Split(' ');
            if (parts.Length == 2 && parts[0] == AuthenticationScheme)
            {
                permissionKey = parts[1];
            }
        }

        var permissions = IntegrationTestWebAppFactory.GetPermissions(permissionKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new("sub", Guid.NewGuid().ToString())
        };

        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permissions", permission));
        }

        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
