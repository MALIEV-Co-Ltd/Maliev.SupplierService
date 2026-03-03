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
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("permissions", "supplier.suppliers.read"),
            new Claim("permissions", "supplier.suppliers.create"),
            new Claim("permissions", "supplier.suppliers.update"),
            new Claim("permissions", "supplier.suppliers.delete"),
            new Claim("permissions", "supplier.suppliers.approve"),
            new Claim("permissions", "supplier.contacts.create"),
            new Claim("permissions", "supplier.certifications.manage"),
            new Claim("permissions", "supplier.performance.rate"),
            new Claim("permissions", "supplier.performance.view")
        };

        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
