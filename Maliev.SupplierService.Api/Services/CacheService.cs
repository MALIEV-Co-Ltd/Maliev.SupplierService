using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Maliev.SupplierService.Api.Services;

/// <summary>
/// Provides caching functionalities using distributed cache (Redis) or in-memory cache as a fallback.
/// </summary>
public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<CacheService> _logger;

    // TTL strategies per research.md
    private static readonly TimeSpan DefaultSupplierTtl = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan DefaultListTtl = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan DefaultEligibilityTtl = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheService"/> class.
    /// </summary>
    /// <param name="cache">The distributed cache instance.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="redis">Optional: The Redis connection multiplexer for advanced operations like tag invalidation.</param>
    public CacheService(
        IDistributedCache cache,
        ILogger<CacheService> logger,
        IConnectionMultiplexer? redis = null)
    {
        _cache = cache;
        _logger = logger;
        _redis = redis;
    }

    /// <summary>
    /// Retrieves a value from the cache asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The cached value, or default if not found or an error occurs.</returns>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cached = await _cache.GetStringAsync(key, cancellationToken);
            if (cached is null)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(cached);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get cache key {Key}", key);
            return default;
        }
    }

    /// <summary>
    /// Sets a value in the cache asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the value to set.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="expiration">Optional: The absolute expiration time for the cache entry. If not provided, a default TTL based on key pattern will be used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? GetDefaultTtl(key)
            };

            await _cache.SetStringAsync(key, json, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set cache key {Key}", key);
        }
    }

    /// <summary>
    /// Removes a value from the cache asynchronously.
    /// </summary>
    /// <param name="key">The cache key to remove.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove cache key {Key}", key);
        }
    }

    /// <summary>
    /// Invalidates all cache entries that match a specific tag asynchronously.
    /// This operation requires Redis to be configured and available.
    /// </summary>
    /// <param name="tag">The tag to invalidate (e.g., "supplier", "suppliers").</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task InvalidateByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        if (_redis is null)
        {
            _logger.LogDebug("Redis not available for tag invalidation: {Tag}", tag);
            return;
        }

        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var db = _redis.GetDatabase();

            // Find keys matching the tag pattern and delete them
            await foreach (var key in server.KeysAsync(pattern: $"{tag}:*"))
            {
                await db.KeyDeleteAsync(key);
            }

            _logger.LogDebug("Invalidated cache keys with tag: {Tag}", tag);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate cache by tag {Tag}", tag);
        }
    }

    private static TimeSpan GetDefaultTtl(string key) => key switch
    {
        _ when key.StartsWith("supplier:") => DefaultSupplierTtl,
        _ when key.StartsWith("suppliers:") => DefaultListTtl,
        _ when key.Contains("eligibility") => DefaultEligibilityTtl,
        _ => DefaultSupplierTtl
    };
}
