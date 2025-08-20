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
public class SaleRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly Faker _faker;

    public SaleRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _faker = new Faker("pt_BR");
    }

    [Fact]
    public async Task CreateAsync_ValidSale_ShouldSaveToDatabase()
    {
        // Arrange
        using var scope = _fixture.ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        var repository = new SaleRepository(context);

        var sale = CreateValidSale();

        // Act
        var result = await repository.CreateAsync(sale);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));

        // Verificar se foi salvo no banco
        var savedSale = await context.Sales
            .Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.Id == result.Id);

        savedSale.Should().NotBeNull();
        savedSale!.UserId.Should().Be(sale.UserId);
        savedSale.Status.Should().Be(SaleStatus.Active);
        savedSale.Products.Should().HaveCount(sale.Products.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingSale_ShouldReturnSale()
    {
        // Arrange
        using var scope = _fixture.ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        var repository = new SaleRepository(context);

        var sale = CreateValidSale();
        context.Sales.Add(sale);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(sale.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(sale.Id);
        result.UserId.Should().Be(sale.UserId);
        result.Products.Should().HaveCount(sale.Products.Count);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingSale_ShouldReturnNull()
    {
        // Arrange
        using var scope = _fixture.ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        var repository = new SaleRepository(context);

        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WithSales_ShouldReturnPaginatedSales()
    {
        // Arrange
        using var scope = _fixture.ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        var repository = new SaleRepository(context);

        var sales = new List<Sale>
        {
            CreateValidSale(),
            CreateValidSale(),
            CreateValidSale()
        };

        context.Sales.AddRange(sales);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAsync(1, 10, null, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterOrEqualTo(3);
    }

    [Fact]
    public async Task UpdateAsync_ExistingSale_ShouldUpdateSale()
    {
        // Arrange
        using var scope = _fixture.ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        var repository = new SaleRepository(context);

        var sale = CreateValidSale();
        context.Sales.Add(sale);
        await context.SaveChangesAsync();

        // Modificar a sale
        sale.Status = SaleStatus.Completed;
        sale.UpdatedAt = DateTimeOffset.UtcNow;

        // Act
        await repository.UpdateAsync(sale);

        // Assert
        var updatedSale = await context.Sales.FindAsync(sale.Id);
        updatedSale.Should().NotBeNull();
        updatedSale!.Status.Should().Be(SaleStatus.Completed);
        updatedSale.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_SaleWithProducts_ShouldSaveProductsCorrectly()
    {
        // Arrange
        using var scope = _fixture.ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        var repository = new SaleRepository(context);

        var sale = CreateSaleWithMultipleProducts();

        // Act
        var result = await repository.CreateAsync(sale);

        // Assert
        result.Should().NotBeNull();
        result.Products.Should().HaveCount(3);

        // Verificar se os produtos foram salvos corretamente
        var savedSale = await context.Sales
            .Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.Id == result.Id);

        savedSale.Should().NotBeNull();
        savedSale!.Products.Should().HaveCount(3);
        savedSale.Products.Should().Contain(p => p.Quantity == 5);
        savedSale.Products.Should().Contain(p => p.Quantity == 10);
        savedSale.Products.Should().Contain(p => p.Quantity == 15);
    }

    [Fact]
    public async Task GetByIdAsync_SaleWithProducts_ShouldIncludeProducts()
    {
        // Arrange
        using var scope = _fixture.ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        var repository = new SaleRepository(context);

        var sale = CreateSaleWithMultipleProducts();
        context.Sales.Add(sale);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(sale.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Products.Should().HaveCount(3);
        result.Products.Should().Contain(p => p.Quantity == 5);
        result.Products.Should().Contain(p => p.Quantity == 10);
        result.Products.Should().Contain(p => p.Quantity == 15);
    }

    [Fact]
    public async Task GetTotalAsync_WithSales_ShouldReturnCorrectCount()
    {
        // Arrange
        using var scope = _fixture.ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        var repository = new SaleRepository(context);

        var sales = new List<Sale>
        {
            CreateValidSale(),
            CreateValidSale(),
            CreateValidSale()
        };

        context.Sales.AddRange(sales);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetTotalAsync(CancellationToken.None);

        // Assert
        result.Should().BeGreaterOrEqualTo(3);
    }

    private Sale CreateValidSale()
    {
        return new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = 1001,
            UserId = Guid.NewGuid(),
            Date = DateTimeOffset.UtcNow,
            Branch = "Loja Centro",
            Status = SaleStatus.Active,
            Products = new List<SaleProduct>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    SaleId = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    Quantity = 2,
                    UnitPrice = 10.0m,
                    Discounts = 1.0m,
                    TotalAmount = 20.0m,
                    Status = SaleProductStatus.Active,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            },
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private Sale CreateSaleWithMultipleProducts()
    {
        var saleId = Guid.NewGuid();
        return new Sale
        {
            Id = saleId,
            SaleNumber = 1002,
            UserId = Guid.NewGuid(),
            Date = DateTimeOffset.UtcNow,
            Branch = "Loja Centro",
            Status = SaleStatus.Active,
            Products = new List<SaleProduct>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    SaleId = saleId,
                    ProductId = Guid.NewGuid(),
                    Quantity = 5,
                    UnitPrice = 10.0m,
                    Discounts = 0.9m,
                    TotalAmount = 45.0m,
                    Status = SaleProductStatus.Active,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    SaleId = saleId,
                    ProductId = Guid.NewGuid(),
                    Quantity = 10,
                    UnitPrice = 15.0m,
                    Discounts = 0.8m,
                    TotalAmount = 120.0m,
                    Status = SaleProductStatus.Active,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    SaleId = saleId,
                    ProductId = Guid.NewGuid(),
                    Quantity = 15,
                    UnitPrice = 8.0m,
                    Discounts = 0.8m,
                    TotalAmount = 96.0m,
                    Status = SaleProductStatus.Active,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            },
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
