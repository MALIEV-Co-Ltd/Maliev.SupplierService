using Maliev.SupplierService.Api.Middleware;

namespace Maliev.SupplierService.Api.Extensions;

/// <summary>
/// Provides extension methods for configuring middleware specific to the supplier service.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Adds common middleware for the supplier service to the application's request pipeline,
    /// including exception handling and request logging.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The configured <see cref="WebApplication"/>.</returns>
    public static WebApplication UseSupplierServiceMiddleware(this WebApplication app)
    {
        // Exception handling should be first
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // Request logging
        app.UseMiddleware<RequestLoggingMiddleware>();

        return app;
    }
}
