namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct);
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct);
    Task RemoveByPrefixAsync(string prefix, CancellationToken ct);
}