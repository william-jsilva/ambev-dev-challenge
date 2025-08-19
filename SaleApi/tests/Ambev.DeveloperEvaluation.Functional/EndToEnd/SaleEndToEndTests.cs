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
public class SaleEndToEndTests : IClassFixture<WebApplicationFactory<Ambev.DeveloperEvaluation.WebApi.Program>>
{
    private readonly WebApplicationFactory<Ambev.DeveloperEvaluation.WebApi.Program> _factory;
    private readonly Faker _faker;
    private readonly HttpClient _client;

    public SaleEndToEndTests(WebApplicationFactory<Ambev.DeveloperEvaluation.WebApi.Program> factory)
    {
        _factory = factory;
        _faker = new Faker("pt_BR");
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateSale_ValidRequest_ShouldReturnCreatedSale()
    {
        // Arrange
        var cart = await CreateCartInDatabase();
        var request = new
        {
            cartId = cart.Id,
            date = DateTimeOffset.UtcNow,
            branch = "Loja Centro"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<SaleResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.CartId.Should().Be(cart.Id);
        result.Branch.Should().Be("Loja Centro");
        result.Status.Should().Be(SaleStatus.Active.ToString());
    }

    [Fact]
    public async Task CreateSale_InvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new
        {
            cartId = Guid.Empty, // Invalid
            date = DateTimeOffset.UtcNow,
            branch = "" // Empty branch
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("CartId");
        errorContent.Should().Contain("Branch");
    }

    [Fact]
    public async Task GetSale_ExistingSale_ShouldReturnSale()
    {
        // Arrange
        var sale = await CreateSaleInDatabase();

        // Act
        var response = await _client.GetAsync($"/api/sales/{sale.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<SaleResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(sale.Id);
        result.UserId.Should().Be(sale.UserId);
        result.Products.Should().HaveCount(sale.Products.Count);
    }

    [Fact]
    public async Task GetSale_NonExistingSale_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/sales/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListSales_WithSales_ShouldReturnSales()
    {
        // Arrange
        await CreateSaleInDatabase();
        await CreateSaleInDatabase();
        await CreateSaleInDatabase();

        // Act
        var response = await _client.GetAsync("/api/sales");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<SaleListResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public async Task UpdateSale_ValidRequest_ShouldUpdateSale()
    {
        // Arrange
        var sale = await CreateSaleInDatabase();
        var updateRequest = new
        {
            userId = sale.UserId,
            date = DateTimeOffset.UtcNow,
            branch = "Loja Norte",
            status = SaleStatus.Completed.ToString(),
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
        var response = await _client.PutAsJsonAsync($"/api/sales/{sale.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<SaleResponse>();
        result.Should().NotBeNull();
        result!.Status.Should().Be(SaleStatus.Completed.ToString());
        result.Branch.Should().Be("Loja Norte");
        result.Products.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteSale_ExistingSale_ShouldMarkAsDeleted()
    {
        // Arrange
        var sale = await CreateSaleInDatabase();

        // Act
        var response = await _client.DeleteAsync($"/api/sales/{sale.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify sale is marked as deleted
        var getResponse = await _client.GetAsync($"/api/sales/{sale.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateSale_WithInvalidCartId_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new
        {
            cartId = Guid.NewGuid(), // Non-existing cart
            date = DateTimeOffset.UtcNow,
            branch = "Loja Centro"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Cart not found");
    }

    [Fact]
    public async Task CreateSale_WithMultipleProducts_ShouldHandleAllProducts()
    {
        // Arrange
        var cart = await CreateCartWithMultipleProductsInDatabase();
        var request = new
        {
            cartId = cart.Id,
            date = DateTimeOffset.UtcNow,
            branch = "Loja Centro"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<SaleResponse>();
        result.Should().NotBeNull();
        result!.Products.Should().HaveCount(3);
        result.Products.Should().Contain(p => p.Quantity == 5);
        result.Products.Should().Contain(p => p.Quantity == 10);
        result.Products.Should().Contain(p => p.Quantity == 15);
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
                    Quantity = 5,
                    UnitPrice = 10.0m
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 3,
                    UnitPrice = 5.0m
                }
            }
        };

        context.Carts.Add(cart);
        await context.SaveChangesAsync();

        return cart;
    }

    private async Task<Cart> CreateCartWithMultipleProductsInDatabase()
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
                new() { ProductId = Guid.NewGuid(), Quantity = 5, UnitPrice = 10.0m },
                new() { ProductId = Guid.NewGuid(), Quantity = 10, UnitPrice = 15.0m },
                new() { ProductId = Guid.NewGuid(), Quantity = 15, UnitPrice = 8.0m }
            }
        };

        context.Carts.Add(cart);
        await context.SaveChangesAsync();

        return cart;
    }

    private async Task<Sale> CreateSaleInDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();

        var sale = new Sale
        {
            SaleNumber = 1001,
            UserId = Guid.NewGuid(),
            Date = DateTimeOffset.UtcNow,
            Branch = "Loja Centro",
            Status = SaleStatus.Active,
            Products = new List<SaleProduct>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 5,
                    UnitPrice = 10.0m,
                    Discounts = 0.9m,
                    TotalAmount = 45.0m,
                    Status = SaleProductStatus.Active
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 3,
                    UnitPrice = 5.0m,
                    Discounts = 1.0m,
                    TotalAmount = 15.0m,
                    Status = SaleProductStatus.Active
                }
            }
        };

        context.Sales.Add(sale);
        await context.SaveChangesAsync();

        return sale;
    }

    // Response models for deserialization
    private class SaleResponse
    {
        public Guid Id { get; set; }
        public Guid CartId { get; set; }
        public Guid UserId { get; set; }
        public DateTimeOffset Date { get; set; }
        public string Branch { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<SaleProductResponse> Products { get; set; } = new();
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }

    private class SaleProductResponse
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discounts { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    private class SaleListResponse
    {
        public List<SaleResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
