using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.ORM.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.ORM.Extensions;

public class QueryParsingExtensionsTests
{
    [Fact]
    public void ParseFrom_WithBasicParameters_ShouldParseCorrectly()
    {
        // Arrange
        var request = CreateMockRequest(new Dictionary<string, string>
        {
            { "_page", "2" },
            { "_size", "20" },
            { "_order", "name,-price" }
        });

        // Act
        var result = QueryParsingExtensions.ParseFrom(request, 1, 10, 100);

        // Assert
        result.Page.Should().Be(2);
        result.Size.Should().Be(20);
        result.Order.Should().Be("name,-price");
    }

    [Fact]
    public void ParseFrom_WithEqualityFilters_ShouldParseCorrectly()
    {
        // Arrange
        var request = CreateMockRequest(new Dictionary<string, string>
        {
            { "Category", "Electronics" },
            { "Title", "iPhone" }
        });

        // Act
        var result = QueryParsingExtensions.ParseFrom(request, 1, 10, 100);

        // Assert
        result.Filters.Should().ContainKey("Category");
        result.Filters["Category"].Should().Be("Electronics");
        result.Filters.Should().ContainKey("Title");
        result.Filters["Title"].Should().Be("iPhone");
    }

    [Fact]
    public void ParseFrom_WithWildcardFilters_ShouldParseCorrectly()
    {
        // Arrange
        var request = CreateMockRequest(new Dictionary<string, string>
        {
            { "Title", "iPhone*" },
            { "Description", "*wireless*" }
        });

        // Act
        var result = QueryParsingExtensions.ParseFrom(request, 1, 10, 100);

        // Assert
        result.Filters.Should().ContainKey("Title");
        result.Filters["Title"].Should().Be("iPhone*");
        result.Filters.Should().ContainKey("Description");
        result.Filters["Description"].Should().Be("*wireless*");
    }

    [Fact]
    public void ParseFrom_WithInvalidPage_ShouldUseDefault()
    {
        // Arrange
        var request = CreateMockRequest(new Dictionary<string, string>
        {
            { "_page", "invalid" }
        });

        // Act
        var result = QueryParsingExtensions.ParseFrom(request, 1, 10, 100);

        // Assert
        result.Page.Should().Be(1); // Default value
    }

    [Fact]
    public void ParseFrom_WithInvalidSize_ShouldUseDefault()
    {
        // Arrange
        var request = CreateMockRequest(new Dictionary<string, string>
        {
            { "_size", "invalid" }
        });

        // Act
        var result = QueryParsingExtensions.ParseFrom(request, 1, 10, 100);

        // Assert
        result.Size.Should().Be(10); // Default value
    }

    [Fact]
    public void ParseFrom_WithSizeExceedingMax_ShouldUseMaxSize()
    {
        // Arrange
        var request = CreateMockRequest(new Dictionary<string, string>
        {
            { "_size", "150" }
        });

        // Act
        var result = QueryParsingExtensions.ParseFrom(request, 1, 10, 100);

        // Assert
        result.Size.Should().Be(100); // Max size
    }

    [Fact]
    public void ParseFrom_WithEmptyRequest_ShouldUseDefaults()
    {
        // Arrange
        var request = CreateMockRequest(new Dictionary<string, string>());

        // Act
        var result = QueryParsingExtensions.ParseFrom(request, 1, 10, 100);

        // Assert
        result.Page.Should().Be(1);
        result.Size.Should().Be(10);
        result.Order.Should().Be(string.Empty);
        result.Filters.Should().BeEmpty();
    }

    private static HttpRequest CreateMockRequest(Dictionary<string, string> queryParams)
    {
        var httpContext = new DefaultHttpContext();
        var request = httpContext.Request;

        var queryCollection = new QueryCollection(
            queryParams.ToDictionary(
                kvp => kvp.Key,
                kvp => new StringValues(kvp.Value)
            )
        );

        request.Query = queryCollection;
        return request;
    }
}
