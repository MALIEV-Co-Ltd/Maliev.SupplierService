namespace Maliev.SupplierService.Api.Configuration;

/// <summary>
/// Defines the settings required for JSON Web Token (JWT) validation.
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// The name of the configuration section for JWT settings.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Gets or sets the public key used to validate the JWT signature.
    /// </summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expected issuer of the JWT.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expected audience of the JWT.
    /// </summary>
    public string Audience { get; set; } = string.Empty;
}
