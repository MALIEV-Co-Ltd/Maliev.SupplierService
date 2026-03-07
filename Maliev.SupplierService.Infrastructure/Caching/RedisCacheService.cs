using System.Text.Json;
using System.Text.Json.Serialization;
using Maliev.SupplierService.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Maliev.SupplierService.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var cachedData = await _cache.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrEmpty(cachedData))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(cachedData, SerializerOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize cache for key {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };

        var serializedData = JsonSerializer.Serialize(value, SerializerOptions);
        await _cache.SetStringAsync(key, serializedData, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // IDistributedCache does not support pattern-based removal natively.
        // In a real MALIEV service, this would use IConnectionMultiplexer to SCAN and DEL.
        // For migration purposes, we log this limitation and remove the literal key if it's not a wildcard.
        _logger.LogWarning("RemoveByPatternAsync called with pattern {Pattern}. IDistributedCache fallback used.", pattern);

        if (!pattern.Contains('*'))
        {
            await _cache.RemoveAsync(pattern, cancellationToken);
        }
    }
}
