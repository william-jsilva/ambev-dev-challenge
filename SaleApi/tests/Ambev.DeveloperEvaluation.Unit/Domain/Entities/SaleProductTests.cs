using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleProductTests
{
    [Theory]
    [InlineData(1, 1.0)]   // Sem desconto
    [InlineData(2, 1.0)]   // Sem desconto
    [InlineData(3, 1.0)]   // Sem desconto
    [InlineData(4, 0.9)]   // 10% desconto
    [InlineData(5, 0.9)]   // 10% desconto
    [InlineData(9, 0.9)]   // 10% desconto
    [InlineData(10, 0.8)]  // 20% desconto
    [InlineData(15, 0.8)]  // 20% desconto
    [InlineData(20, 0.8)]  // 20% desconto
    public void CalculateDiscount_WithDifferentQuantities_ShouldApplyCorrectDiscount(int quantity, decimal expectedDiscount)
    {
        // Arrange
        var saleProduct = new SaleProduct
        {
            Quantity = quantity,
            UnitPrice = 10.0m
        };

        // Act
        saleProduct.CalculateDiscount();

        // Assert
        saleProduct.Discounts.Should().Be(expectedDiscount);
    }

    [Fact]
    public void CalculateTotalAmount_WithDiscount_ShouldCalculateCorrectTotal()
    {
        // Arrange
        var saleProduct = new SaleProduct
        {
            Quantity = 5,
            UnitPrice = 10.0m
        };

        // Act
        saleProduct.CalculateDiscount();
        saleProduct.CalculateTotalAmount();

        // Assert
        // 5 * 10.0 * 0.9 = 45.0
        saleProduct.TotalAmount.Should().Be(45.0m);
    }

    [Fact]
    public void CalculateTotalAmount_WithoutDiscount_ShouldCalculateCorrectTotal()
    {
        // Arrange
        var saleProduct = new SaleProduct
        {
            Quantity = 2,
            UnitPrice = 10.0m
        };

        // Act
        saleProduct.CalculateDiscount();
        saleProduct.CalculateTotalAmount();

        // Assert
        // 2 * 10.0 * 1.0 = 20.0
        saleProduct.TotalAmount.Should().Be(20.0m);
    }

    [Fact]
    public void CalculateTotalAmount_WithMaximumDiscount_ShouldCalculateCorrectTotal()
    {
        // Arrange
        var saleProduct = new SaleProduct
        {
            Quantity = 15,
            UnitPrice = 10.0m
        };

        // Act
        saleProduct.CalculateDiscount();
        saleProduct.CalculateTotalAmount();

        // Assert
        // 15 * 10.0 * 0.8 = 120.0
        saleProduct.TotalAmount.Should().Be(120.0m);
    }

    [Fact]
    public void CalculateDiscount_WithZeroQuantity_ShouldApplyNoDiscount()
    {
        // Arrange
        var saleProduct = new SaleProduct
        {
            Quantity = 0,
            UnitPrice = 10.0m
        };

        // Act
        saleProduct.CalculateDiscount();

        // Assert
        saleProduct.Discounts.Should().Be(1.0m);
    }

    [Fact]
    public void CalculateDiscount_WithNegativeQuantity_ShouldApplyNoDiscount()
    {
        // Arrange
        var saleProduct = new SaleProduct
        {
            Quantity = -5,
            UnitPrice = 10.0m
        };

        // Act
        saleProduct.CalculateDiscount();

        // Assert
        saleProduct.Discounts.Should().Be(1.0m);
    }

    [Fact]
    public void CalculateTotalAmount_WithDecimalUnitPrice_ShouldCalculateCorrectTotal()
    {
        // Arrange
        var saleProduct = new SaleProduct
        {
            Quantity = 6,
            UnitPrice = 7.50m
        };

        // Act
        saleProduct.CalculateDiscount();
        saleProduct.CalculateTotalAmount();

        // Assert
        // 6 * 7.50 * 0.9 = 40.5
        saleProduct.TotalAmount.Should().Be(40.5m);
    }

    [Fact]
    public void DefineDeleted_ShouldUpdateStatusAndTimestamps()
    {
        // Arrange
        var saleProduct = new SaleProduct
        {
            Status = SaleProductStatus.Active
        };

        var beforeDelete = DateTimeOffset.UtcNow;

        // Act
        saleProduct.DefineDeleted();

        // Assert
        saleProduct.Status.Should().Be(SaleProductStatus.Deleted);
        saleProduct.DeletedAt.Should().NotBeNull();
        saleProduct.UpdatedAt.Should().NotBeNull();
        saleProduct.DeletedAt.Should().BeAfter(beforeDelete);
        saleProduct.UpdatedAt.Should().BeAfter(beforeDelete);
    }

    [Theory]
    [InlineData(1, 10.0, 10.0)]      // Sem desconto
    [InlineData(4, 10.0, 36.0)]      // 10% desconto
    [InlineData(10, 10.0, 80.0)]     // 20% desconto
    [InlineData(20, 10.0, 160.0)]    // 20% desconto
    public void CalculateTotalAmount_WithVariousScenarios_ShouldCalculateCorrectTotal(int quantity, decimal unitPrice, decimal expectedTotal)
    {
        // Arrange
        var saleProduct = new SaleProduct
        {
            Quantity = quantity,
            UnitPrice = unitPrice
        };

        // Act
        saleProduct.CalculateDiscount();
        saleProduct.CalculateTotalAmount();

        // Assert
        saleProduct.TotalAmount.Should().Be(expectedTotal);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var saleProduct = new SaleProduct();

        // Assert
        saleProduct.Status.Should().Be(SaleProductStatus.Active);
        saleProduct.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }
}
