using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.ORM.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private const string KEY_INDEX_PREFIX = "key_index:";

    public RedisCacheService(IDistributedCache cache) => _cache = cache;

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct)
    {
        var str = await _cache.GetStringAsync(key, ct);
        return str is null ? default : JsonSerializer.Deserialize<T>(str);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };
        var str = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, str, options, ct);
        
        // Store key in index for prefix invalidation
        await AddKeyToIndex(key, ct);
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct)
    {
        try
        {
            // Get all keys with the prefix from our index
            var indexKey = $"{KEY_INDEX_PREFIX}{prefix}";
            var keysJson = await _cache.GetStringAsync(indexKey, ct);
            
            if (!string.IsNullOrEmpty(keysJson))
            {
                var keys = JsonSerializer.Deserialize<List<string>>(keysJson);
                if (keys != null)
                {
                    foreach (var key in keys)
                    {
                        await _cache.RemoveAsync(key, ct);
                    }
                    // Remove the index itself
                    await _cache.RemoveAsync(indexKey, ct);
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the operation
            Console.WriteLine($"Error during cache invalidation: {ex.Message}");
        }
    }

    private async Task AddKeyToIndex(string key, CancellationToken ct)
    {
        try
        {
            // Extract prefix from key (e.g., "products:list:" from "products:list:p=1:s=10")
            var prefix = ExtractPrefix(key);
            var indexKey = $"{KEY_INDEX_PREFIX}{prefix}";
            
            // Get existing keys
            var keysJson = await _cache.GetStringAsync(indexKey, ct);
            var keys = string.IsNullOrEmpty(keysJson) 
                ? new List<string>() 
                : JsonSerializer.Deserialize<List<string>>(keysJson) ?? new List<string>();
            
            // Add new key if not already present
            if (!keys.Contains(key))
            {
                keys.Add(key);
                var updatedKeysJson = JsonSerializer.Serialize(keys);
                await _cache.SetStringAsync(indexKey, updatedKeysJson, 
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) }, ct);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the operation
            Console.WriteLine($"Error adding key to index: {ex.Message}");
        }
    }

    private static string ExtractPrefix(string key)
    {
        // Extract prefix based on key patterns
        if (key.StartsWith("products:list:"))
            return "products:list:";
        if (key.StartsWith("products:item:"))
            return "products:item:";
        if (key.StartsWith("products:categories"))
            return "products:categories";
        
        // Default: extract everything before the last colon
        var lastColonIndex = key.LastIndexOf(':');
        return lastColonIndex > 0 ? key.Substring(0, lastColonIndex + 1) : key;
    }
}
