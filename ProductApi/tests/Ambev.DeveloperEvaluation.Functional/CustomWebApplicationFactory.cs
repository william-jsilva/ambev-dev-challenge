using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Ambev.DeveloperEvaluation.ORM.Mongo;
using Ambev.DeveloperEvaluation.ORM.Cache;
using Ambev.DeveloperEvaluation.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Functional;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: true);
            config.AddEnvironmentVariables();
        });

        builder.ConfigureServices(services =>
        {
            // Replace real services with test doubles if needed
            // For now, we'll use the real services but with test configuration
            
            // Ensure we're using test database
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(MongoContext));
            
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add test MongoDB context
            services.AddSingleton<MongoContext>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
                var databaseName = configuration["MongoDB:DatabaseName"] ?? "ambev_test";
                
                var options = new MongoOptions { ConnectionString = connectionString, Database = databaseName, Collection = "products" };
                return new MongoContext(options);
            });

            // Replace Redis with in-memory cache for tests
            var cacheDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ICacheService));
            
            if (cacheDescriptor != null)
            {
                services.Remove(cacheDescriptor);
            }

            services.AddMemoryCache();
            services.AddSingleton<ICacheService, InMemoryCacheService>();
        });
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
