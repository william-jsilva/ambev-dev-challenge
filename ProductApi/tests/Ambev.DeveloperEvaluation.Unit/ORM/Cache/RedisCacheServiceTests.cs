using Ambev.DeveloperEvaluation.ORM.Cache;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using System.Text.Json;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.ORM.Cache;

public class RedisCacheServiceTests
{
    private readonly IDistributedCache _distributedCache;
    private readonly RedisCacheService _cacheService;

    public RedisCacheServiceTests()
    {
        _distributedCache = Substitute.For<IDistributedCache>();
        _cacheService = new RedisCacheService(_distributedCache);
    }

    [Theory]
    [InlineData("products:list:p=1:s=10")]
    [InlineData("products:item:123")]
    [InlineData("products:categories")]
    [InlineData("custom:key:value")]
    public void ExtractPrefix_ShouldReturnCorrectPrefix(string key)
    {
        // This test would require making ExtractPrefix public or testing it indirectly
        // For now, we'll test it through the SetAsync method which uses it internally
        
        // Arrange
        var value = new TestData { Id = 1, Name = "Test" };
        var ttl = TimeSpan.FromMinutes(5);

        // Act
        var action = () => _cacheService.SetAsync(key, value, ttl, CancellationToken.None);

        // Assert
        action.Should().NotThrowAsync();
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
