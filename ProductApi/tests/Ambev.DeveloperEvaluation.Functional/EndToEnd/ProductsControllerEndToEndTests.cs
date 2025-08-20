using Ambev.DeveloperEvaluation.Application.Dtos;
using Ambev.DeveloperEvaluation.Functional;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.EndToEnd;

public class ProductsControllerEndToEndTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly Faker<ProductDto> _productDtoFaker;

    public ProductsControllerEndToEndTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        _productDtoFaker = new Faker<ProductDto>()
            .RuleFor(p => p.Title, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => f.Random.Double(1, 1000))
            .RuleFor(p => p.Description, f => f.Lorem.Sentence())
            .RuleFor(p => p.Category, f => f.Commerce.Categories(1)[0])
            .RuleFor(p => p.Image, f => f.Image.PicsumUrl())
            .RuleFor(p => p.Rating, f => new RatingDto
            {
                Rate = f.Random.Double(0, 5),
                Count = f.Random.Int(0, 1000)
            });
    }

    [Fact]
    public async Task GetProducts_ShouldReturnOkWithPaginationResult()
    {
        // Act
        var response = await _client.GetAsync("/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginationResult<ProductDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.TotalItems.Should().BeGreaterThanOrEqualTo(0);
        result.CurrentPage.Should().Be(1);
    }

    [Fact]
    public async Task GetProducts_WithPagination_ShouldReturnCorrectResults()
    {
        // Act
        var response = await _client.GetAsync("/products?_page=1&_size=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginationResult<ProductDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().HaveCountLessThanOrEqualTo(5);
    }

    [Fact]
    public async Task GetProducts_WithOrdering_ShouldReturnOrderedResults()
    {
        // Act
        var response = await _client.GetAsync("/products?_order=Price");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginationResult<ProductDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().BeInAscendingOrder(p => p.Price);
    }

    [Fact]
    public async Task CreateProduct_InvalidPrice_ShouldReturnBadRequest()
    {
        // Arrange
        var productDto = _productDtoFaker.Generate();
        productDto.Price = -10; // Invalid negative price
        var json = JsonSerializer.Serialize(productDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/products", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProductById_NonExistingProduct_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = "non-existing-id";

        // Act
        var response = await _client.GetAsync($"/products/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProductsByCategory_NonExistingCategory_ShouldReturnEmptyResults()
    {
        // Act
        var response = await _client.GetAsync("/products/category/NonExistingCategory");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginationResult<ProductDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().BeEmpty();
    }
}

public class PaginationResult<T>
{
    public IEnumerable<T> Data { get; set; } = [];
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
