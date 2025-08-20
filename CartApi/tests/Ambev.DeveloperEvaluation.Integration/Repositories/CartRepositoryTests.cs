using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Repositories;

[Collection("Database")]
public class CartRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly Faker _faker;

    public CartRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _faker = new Faker("pt_BR");
    }

    [Fact]
    public async Task CreateAsync_ValidCart_ShouldSaveToDatabase()
    {
        // Arrange
        using var scope = _fixture.ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        var repository = new CartRepository(context);

        var cart = CreateValidCart();

        // Act
        var result = await repository.CreateAsync(cart);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));

        // Verificar se foi salvo no banco
        var savedCart = await context.Carts
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == result.Id);

        savedCart.Should().NotBeNull();
        savedCart!.UserId.Should().Be(cart.UserId);
        savedCart.Status.Should().Be(CartStatus.Active);
        savedCart.Products.Should().HaveCount(cart.Products.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingCart_ShouldReturnCart()
    {
        // Arrange
        using var scope = _fixture.ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        var repository = new CartRepository(context);

        var cart = CreateValidCart();
        context.Carts.Add(cart);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(cart.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(cart.Id);
        result.UserId.Should().Be(cart.UserId);
        result.Products.Should().HaveCount(cart.Products.Count);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingCart_ShouldReturnNull()
    {
        // Arrange
        using var scope = _fixture.ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        var repository = new CartRepository(context);

        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WithCarts_ShouldReturnPaginatedCarts()
    {
        // Arrange
        using var scope = _fixture.ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        var repository = new CartRepository(context);

        var carts = new List<Cart>
        {
            CreateValidCart(),
            CreateValidCart(),
            CreateValidCart()
        };

        context.Carts.AddRange(carts);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAsync(1, 10, null, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterOrEqualTo(3);
    }

    [Fact]
    public async Task UpdateAsync_ExistingCart_ShouldUpdateCart()
    {
        // Arrange
        using var scope = _fixture.ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        var repository = new CartRepository(context);

        var cart = CreateValidCart();
        context.Carts.Add(cart);
        await context.SaveChangesAsync();

        // Modificar o cart
        cart.Status = CartStatus.Completed;
        cart.UpdatedAt = DateTimeOffset.UtcNow;

        // Act
        await repository.UpdateAsync(cart);

        // Assert
        var updatedCart = await context.Carts.FindAsync(cart.Id);
        updatedCart.Should().NotBeNull();
        updatedCart!.Status.Should().Be(CartStatus.Completed);
        updatedCart.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_CartWithProducts_ShouldSaveProductsCorrectly()
    {
        // Arrange
        using var scope = _fixture.ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        var repository = new CartRepository(context);

        var cart = CreateCartWithMultipleProducts();

        // Act
        var result = await repository.CreateAsync(cart);

        // Assert
        result.Should().NotBeNull();
        result.Products.Should().HaveCount(3);

        // Verificar se os produtos foram salvos corretamente
        var savedCart = await context.Carts
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == result.Id);

        savedCart.Should().NotBeNull();
        savedCart!.Products.Should().HaveCount(3);
        savedCart.Products.Should().Contain(p => p.Quantity == 5);
        savedCart.Products.Should().Contain(p => p.Quantity == 10);
        savedCart.Products.Should().Contain(p => p.Quantity == 15);
    }

    [Fact]
    public async Task GetByIdAsync_CartWithProducts_ShouldIncludeProducts()
    {
        // Arrange
        using var scope = _fixture.ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        var repository = new CartRepository(context);

        var cart = CreateCartWithMultipleProducts();
        context.Carts.Add(cart);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(cart.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Products.Should().HaveCount(3);
        result.Products.Should().Contain(p => p.Quantity == 5);
        result.Products.Should().Contain(p => p.Quantity == 10);
        result.Products.Should().Contain(p => p.Quantity == 15);
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_ExistingActiveCart_ShouldReturnCart()
    {
        // Arrange
        using var scope = _fixture.ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        var repository = new CartRepository(context);

        var userId = Guid.NewGuid();
        var cart = CreateValidCart();
        cart.UserId = userId;
        cart.Status = CartStatus.Active;

        context.Carts.Add(cart);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetActiveByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.Status.Should().Be(CartStatus.Active);
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_NonExistingUser_ShouldReturnNull()
    {
        // Arrange
        using var scope = _fixture.ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        var repository = new CartRepository(context);

        var nonExistingUserId = Guid.NewGuid();

        // Act
        var result = await repository.GetActiveByUserIdAsync(nonExistingUserId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTotalAsync_WithCarts_ShouldReturnCorrectCount()
    {
        // Arrange
        using var scope = _fixture.ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        var repository = new CartRepository(context);

        var carts = new List<Cart>
        {
            CreateValidCart(),
            CreateValidCart(),
            CreateValidCart()
        };

        context.Carts.AddRange(carts);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetTotalAsync(CancellationToken.None);

        // Assert
        result.Should().BeGreaterOrEqualTo(3);
    }

    private Cart CreateValidCart()
    {
        return new Cart
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Date = DateTimeOffset.UtcNow,
            Status = CartStatus.Active,
            Products = new List<CartProduct>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CartId = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    Quantity = 2,
                    Status = CartProductStatus.Active,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            },
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private Cart CreateCartWithMultipleProducts()
    {
        var cartId = Guid.NewGuid();
        return new Cart
        {
            Id = cartId,
            UserId = Guid.NewGuid(),
            Date = DateTimeOffset.UtcNow,
            Status = CartStatus.Active,
            Products = new List<CartProduct>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    CartId = cartId,
                    ProductId = Guid.NewGuid(),
                    Quantity = 5,
                    Status = CartProductStatus.Active,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CartId = cartId,
                    ProductId = Guid.NewGuid(),
                    Quantity = 10,
                    Status = CartProductStatus.Active,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CartId = cartId,
                    ProductId = Guid.NewGuid(),
                    Quantity = 15,
                    Status = CartProductStatus.Active,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            },
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
