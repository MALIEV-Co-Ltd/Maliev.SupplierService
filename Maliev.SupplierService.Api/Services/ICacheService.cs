namespace Maliev.SupplierService.Api.Services;

/// <summary>
/// Defines the contract for a caching service.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Retrieves a value from the cache asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The cached value, or default if not found or an error occurs.</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value in the cache asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the value to set.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="expiration">Optional: The absolute expiration time for the cache entry. If not provided, a default TTL based on key pattern will be used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a value from the cache asynchronously.
    /// </summary>
    /// <param name="key">The cache key to remove.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all cache entries that match a specific tag asynchronously.
    /// This operation requires Redis to be configured and available.
    /// </summary>
    /// <param name="tag">The tag to invalidate (e.g., "supplier", "suppliers").</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task InvalidateByTagAsync(string tag, CancellationToken cancellationToken = default);
}
