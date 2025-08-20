using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Ambev.DeveloperEvaluation.ORM.Mongo;
using Ambev.DeveloperEvaluation.ORM.Cache;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Integration;

public class DatabaseFixture : IDisposable
{
    public IMongoDatabase Database { get; }
    public IMongoClient Client { get; }
    public IServiceProvider ServiceProvider { get; }

    public DatabaseFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
        var databaseName = configuration["MongoDB:DatabaseName"] ?? "ambev_test";

        Client = new MongoClient(connectionString);
        Database = Client.GetDatabase(databaseName);

        // Setup service provider for dependency injection
        var services = new ServiceCollection();
        services.AddSingleton(new MongoOptions { ConnectionString = connectionString, Database = databaseName, Collection = "products" });
        services.AddSingleton<MongoContext>();
        services.AddSingleton<IProductRepository, ProductRepository>();
        
        // Use in-memory cache for tests instead of Redis
        services.AddMemoryCache();
        services.AddSingleton<ICacheService>(provider => 
        {
            var memoryCache = provider.GetRequiredService<IMemoryCache>();
            return new InMemoryCacheService(memoryCache);
        });
        
        ServiceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        // Clean up test database
        Client.DropDatabase(Database.DatabaseNamespace.DatabaseName);
        Client?.Dispose();
    }
}

// Simple in-memory cache service for testing
public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public InMemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_cache.Get<T>(key));
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };
        
        _cache.Set(key, value, options);
        await Task.CompletedTask;
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct)
    {
        // For in-memory cache, we can't easily remove by prefix
        // In a real scenario, you might want to implement a more sophisticated approach
        await Task.CompletedTask;
    }
}
