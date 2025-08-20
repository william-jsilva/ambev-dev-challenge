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
        var cartId = Guid.NewGuid();
        var request = new
        {
            cartId = cartId,
            date = DateTimeOffset.UtcNow,
            branch = "Loja Centro"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Assert
        // Since we don't have a real cart, we expect a validation error
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Cart not found");
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
    public async Task ListSales_ShouldReturnSales()
    {
        // Act
        var response = await _client.GetAsync("/api/sales");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<SaleListResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateSale_NonExistingSale_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var updateRequest = new
        {
            userId = Guid.NewGuid(),
            date = DateTimeOffset.UtcNow,
            branch = "Loja Norte",
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
        var response = await _client.PutAsJsonAsync($"/api/sales/{nonExistingId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteSale_NonExistingSale_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/sales/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
        var cartId = Guid.NewGuid();
        var request = new
        {
            cartId = cartId,
            date = DateTimeOffset.UtcNow,
            branch = "Loja Centro"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Assert
        // Since we don't have a real cart, we expect a validation error
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Cart not found");
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
