namespace Maliev.SupplierService.Api.Configuration;

/// <summary>
/// Defines the settings for connecting to a Redis cache.
/// </summary>
public class RedisSettings
{
    /// <summary>
    /// The name of the configuration section for Redis settings.
    /// </summary>
    public const string SectionName = "Redis";

    /// <summary>
    /// Gets or sets the connection string for the Redis server.
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Gets or sets a value indicating whether the Redis cache is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;
}
