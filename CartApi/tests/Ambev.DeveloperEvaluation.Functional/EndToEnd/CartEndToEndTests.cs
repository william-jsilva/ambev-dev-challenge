using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.ORM;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Ambev.DeveloperEvaluation.WebApi;

namespace Ambev.DeveloperEvaluation.Functional.EndToEnd;

[Collection("WebApplicationFactory")]
public class CartEndToEndTests : IClassFixture<WebApplicationFactory<Ambev.DeveloperEvaluation.WebApi.Program>>
{
    private readonly WebApplicationFactory<Ambev.DeveloperEvaluation.WebApi.Program> _factory;
    private readonly Faker _faker;
    private readonly HttpClient _client;

    public CartEndToEndTests(WebApplicationFactory<Ambev.DeveloperEvaluation.WebApi.Program> factory)
    {
        _factory = factory;
        _faker = new Faker("pt_BR");
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateCart_ValidRequest_ShouldReturnCreatedCart()
    {
        // Arrange
        var request = new
        {
            userId = Guid.NewGuid(),
            date = DateTimeOffset.UtcNow,
            products = new[]
            {
                new
                {
                    productId = Guid.NewGuid(),
                    quantity = 5,
                    unitPrice = 10.0m
                },
                new
                {
                    productId = Guid.NewGuid(),
                    quantity = 3,
                    unitPrice = 5.0m
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/carts", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<CartResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.UserId.Should().Be(request.userId);
        result.Products.Should().HaveCount(2);
        result.Status.Should().Be(CartStatus.Active.ToString());
    }

    [Fact]
    public async Task CreateCart_InvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new
        {
            userId = Guid.Empty, // Invalid
            date = DateTimeOffset.UtcNow,
            products = new object[] { } // Empty products
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/carts", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("UserId");
        errorContent.Should().Contain("Products");
    }

    [Fact]
    public async Task GetCart_ExistingCart_ShouldReturnCart()
    {
        // Arrange
        var cart = await CreateCartInDatabase();

        // Act
        var response = await _client.GetAsync($"/api/carts/{cart.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<CartResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(cart.Id);
        result.UserId.Should().Be(cart.UserId);
        result.Products.Should().HaveCount(cart.Products.Count);
    }

    [Fact]
    public async Task GetCart_NonExistingCart_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/carts/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListCarts_WithCarts_ShouldReturnCarts()
    {
        // Arrange
        await CreateCartInDatabase();
        await CreateCartInDatabase();
        await CreateCartInDatabase();

        // Act
        var response = await _client.GetAsync("/api/carts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<CartListResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public async Task UpdateCart_ValidRequest_ShouldUpdateCart()
    {
        // Arrange
        var cart = await CreateCartInDatabase();
        var updateRequest = new
        {
            userId = cart.UserId,
            date = DateTimeOffset.UtcNow,
            status = CartStatus.Completed.ToString(),
            products = new[]
            {
                new
                {
                    productId = Guid.NewGuid(),
                    quantity = 10,
                    unitPrice = 15.0m
                }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/carts/{cart.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<CartResponse>();
        result.Should().NotBeNull();
        result!.Status.Should().Be(CartStatus.Completed.ToString());
        result.Products.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteCart_ExistingCart_ShouldMarkAsDeleted()
    {
        // Arrange
        var cart = await CreateCartInDatabase();

        // Act
        var response = await _client.DeleteAsync($"/api/carts/{cart.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify cart is marked as deleted
        var getResponse = await _client.GetAsync($"/api/carts/{cart.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCart_WithInvalidProductQuantity_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new
        {
            userId = Guid.NewGuid(),
            date = DateTimeOffset.UtcNow,
            products = new[]
            {
                new
                {
                    productId = Guid.NewGuid(),
                    quantity = 0, // Invalid quantity
                    unitPrice = 10.0m
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/carts", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Quantity");
    }

    [Fact]
    public async Task CreateCart_WithNegativeUnitPrice_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new
        {
            userId = Guid.NewGuid(),
            date = DateTimeOffset.UtcNow,
            products = new[]
            {
                new
                {
                    productId = Guid.NewGuid(),
                    quantity = 5,
                    unitPrice = -10.0m // Invalid price
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/carts", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("UnitPrice");
    }

    [Fact]
    public async Task CreateCart_WithMultipleProducts_ShouldHandleAllProducts()
    {
        // Arrange
        var request = new
        {
            userId = Guid.NewGuid(),
            date = DateTimeOffset.UtcNow,
            products = new[]
            {
                new { productId = Guid.NewGuid(), quantity = 5 },
                new { productId = Guid.NewGuid(), quantity = 10 },
                new { productId = Guid.NewGuid(), quantity = 15 },
                new { productId = Guid.NewGuid(), quantity = 2 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/carts", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<CartResponse>();
        result.Should().NotBeNull();
        result!.Products.Should().HaveCount(4);
        result.Products.Should().Contain(p => p.Quantity == 5);
        result.Products.Should().Contain(p => p.Quantity == 10);
        result.Products.Should().Contain(p => p.Quantity == 15);
        result.Products.Should().Contain(p => p.Quantity == 2);
    }

    private async Task<Cart> CreateCartInDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();

        var cart = new Cart
        {
            UserId = Guid.NewGuid(),
            Date = DateTimeOffset.UtcNow,
            Status = CartStatus.Active,
            Products = new List<CartProduct>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 5
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 3
                }
            }
        };

        context.Carts.Add(cart);
        await context.SaveChangesAsync();

        return cart;
    }

    // Response models for deserialization
    private class CartResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTimeOffset Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<CartProductResponse> Products { get; set; } = new();
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }

    private class CartProductResponse
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }

        public string Status { get; set; } = string.Empty;
    }

    private class CartListResponse
    {
        public List<CartResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
