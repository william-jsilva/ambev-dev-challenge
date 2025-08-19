namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Data Transfer Object representing a product in a cart
/// </summary>
public class CartProduct
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
}
