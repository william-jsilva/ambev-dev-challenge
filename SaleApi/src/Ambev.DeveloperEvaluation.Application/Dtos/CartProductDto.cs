namespace Ambev.DeveloperEvaluation.Application.Dtos;

/// <summary>
/// Data Transfer Object representing a product in a sale
/// </summary>
public class SaleProductDto
{
    /// <summary>
    /// Product id
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Quantity of the product
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Unit price of the product
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Discounts applied to the product
    /// </summary>
    public decimal Discounts { get; set; }

    /// <summary>
    /// Total amount of the product (Quantity * UnitPrice - Discounts)
    /// </summary>
    public decimal TotalAmount { get; set; }
}
