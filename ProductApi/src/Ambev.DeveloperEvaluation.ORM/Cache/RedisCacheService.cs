using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.ORM.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;

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
    }

    // naive prefix invalidation (requires a list of keys per prefix; you can improve with Redis SCAN if allowed)
    public Task RemoveByPrefixAsync(string prefix, CancellationToken ct)
    {
        // Implementação avançada: manter um índice de chaves por prefixo em um Set. Aqui, deixe como no-op ou expanda depois.
        return Task.CompletedTask;
    }
}
