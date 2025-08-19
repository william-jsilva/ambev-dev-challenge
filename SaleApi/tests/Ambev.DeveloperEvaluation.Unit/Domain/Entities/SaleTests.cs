using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    [Fact]
    public void CalculateTotalSaleAmount_WithMultipleProducts_ShouldCalculateCorrectTotal()
    {
        // Arrange
        var sale = new Sale
        {
            Products = new List<SaleProduct>
            {
                new() { Quantity = 5, UnitPrice = 10.0m },  // 5 * 10 * 0.9 = 45
                new() { Quantity = 3, UnitPrice = 5.0m },   // 3 * 5 * 1.0 = 15
                new() { Quantity = 12, UnitPrice = 8.0m }   // 12 * 8 * 0.8 = 76.8
            }
        };

        // Act
        sale.CalculateTotalSaleAmount();

        // Assert
        // Total: 45 + 15 + 76.8 = 136.8
        sale.TotalSaleAmount.Should().Be(136.8m);
    }

    [Fact]
    public void CalculateTotalSaleAmount_WithNoProducts_ShouldReturnZero()
    {
        // Arrange
        var sale = new Sale
        {
            Products = new List<SaleProduct>()
        };

        // Act
        sale.CalculateTotalSaleAmount();

        // Assert
        sale.TotalSaleAmount.Should().Be(0m);
    }

    [Fact]
    public void CalculateTotalSaleAmount_WithSingleProduct_ShouldCalculateCorrectTotal()
    {
        // Arrange
        var sale = new Sale
        {
            Products = new List<SaleProduct>
            {
                new() { Quantity = 10, UnitPrice = 15.0m }  // 10 * 15 * 0.8 = 120
            }
        };

        // Act
        sale.CalculateTotalSaleAmount();

        // Assert
        sale.TotalSaleAmount.Should().Be(120.0m);
    }

    [Fact]
    public void DefineDeleted_ShouldUpdateStatusAndTimestamps()
    {
        // Arrange
        var sale = new Sale
        {
            Status = SaleStatus.Active
        };

        var beforeDelete = DateTimeOffset.UtcNow;

        // Act
        sale.DefineDeleted();

        // Assert
        sale.Status.Should().Be(SaleStatus.Deleted);
        sale.DeletedAt.Should().NotBeNull();
        sale.UpdatedAt.Should().NotBeNull();
        sale.DeletedAt.Should().BeAfter(beforeDelete);
        sale.UpdatedAt.Should().BeAfter(beforeDelete);
    }

    [Fact]
    public void IsCompleted_WithCompletedStatus_ShouldReturnTrue()
    {
        // Arrange
        var sale = new Sale
        {
            Status = SaleStatus.Completed
        };

        // Act
        var result = sale.IsCompleted();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(SaleStatus.Active)]
    [InlineData(SaleStatus.Cancelled)]
    [InlineData(SaleStatus.Deleted)]
    public void IsCompleted_WithNonCompletedStatus_ShouldReturnFalse(SaleStatus status)
    {
        // Arrange
        var sale = new Sale
        {
            Status = status
        };

        // Act
        var result = sale.IsCompleted();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithValidSale_ShouldReturnValidResult()
    {
        // Arrange
        var sale = new Sale
        {
            SaleNumber = 1001,
            Branch = "Loja Centro",
            UserId = Guid.NewGuid(),
            Date = DateTimeOffset.UtcNow,
            Status = SaleStatus.Active
        };

        // Act
        var result = sale.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithInvalidSaleNumber_ShouldReturnValidationError()
    {
        // Arrange
        var sale = new Sale
        {
            SaleNumber = 0,
            Branch = "Loja Centro",
            UserId = Guid.NewGuid(),
            Date = DateTimeOffset.UtcNow
        };

        // Act
        var result = sale.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Error.Should().Contain("SaleNumber");
    }

    [Fact]
    public void Validate_WithEmptyBranch_ShouldReturnValidationError()
    {
        // Arrange
        var sale = new Sale
        {
            SaleNumber = 1001,
            Branch = "",
            UserId = Guid.NewGuid(),
            Date = DateTimeOffset.UtcNow
        };

        // Act
        var result = sale.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Error.Should().Contain("Branch");
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldReturnValidationError()
    {
        // Arrange
        var sale = new Sale
        {
            SaleNumber = 1001,
            Branch = "Loja Centro",
            UserId = Guid.Empty,
            Date = DateTimeOffset.UtcNow
        };

        // Act
        var result = sale.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Error.Should().Contain("UserId");
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var sale = new Sale();

        // Assert
        sale.Status.Should().Be(SaleStatus.Active);
        sale.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        sale.Products.Should().NotBeNull();
    }

    [Fact]
    public void CalculateTotalSaleAmount_WithProductsHavingDifferentDiscounts_ShouldCalculateCorrectTotal()
    {
        // Arrange
        var sale = new Sale
        {
            Products = new List<SaleProduct>
            {
                new() { Quantity = 2, UnitPrice = 20.0m },   // Sem desconto: 2 * 20 * 1.0 = 40
                new() { Quantity = 6, UnitPrice = 15.0m },   // 10% desconto: 6 * 15 * 0.9 = 81
                new() { Quantity = 15, UnitPrice = 10.0m }   // 20% desconto: 15 * 10 * 0.8 = 120
            }
        };

        // Act
        sale.CalculateTotalSaleAmount();

        // Assert
        // Total: 40 + 81 + 120 = 241
        sale.TotalSaleAmount.Should().Be(241.0m);
    }

    [Fact]
    public void CalculateTotalSaleAmount_ShouldUpdateAllProductCalculations()
    {
        // Arrange
        var sale = new Sale
        {
            Products = new List<SaleProduct>
            {
                new() { Quantity = 5, UnitPrice = 10.0m },
                new() { Quantity = 12, UnitPrice = 8.0m }
            }
        };

        // Act
        sale.CalculateTotalSaleAmount();

        // Assert
        sale.Products[0].Discounts.Should().Be(0.9m);  // 10% desconto
        sale.Products[0].TotalAmount.Should().Be(45.0m); // 5 * 10 * 0.9
        sale.Products[1].Discounts.Should().Be(0.8m);  // 20% desconto
        sale.Products[1].TotalAmount.Should().Be(76.8m); // 12 * 8 * 0.8
    }
}
