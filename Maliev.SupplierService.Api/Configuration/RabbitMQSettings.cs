namespace Maliev.SupplierService.Api.Configuration;

/// <summary>
/// Defines the settings for connecting to a RabbitMQ message broker.
/// </summary>
public class RabbitMQSettings
{
    /// <summary>
    /// The name of the configuration section for RabbitMQ settings.
    /// </summary>
    public const string SectionName = "RabbitMQ";

    /// <summary>
    /// Gets or sets a value indicating whether the RabbitMQ connection is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the RabbitMQ host name.
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets the RabbitMQ port.
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Gets or sets the username for the RabbitMQ connection.
    /// </summary>
    public string Username { get; set; } = "guest";

    /// <summary>
    /// Gets or sets the password for the RabbitMQ connection.
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Gets or sets the virtual host for the RabbitMQ connection.
    /// </summary>
    public string VirtualHost { get; set; } = "/";
}
