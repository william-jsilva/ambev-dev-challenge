namespace Ambev.DeveloperEvaluation.Application.Dtos;

/// <summary>
/// Data Transfer Object representing a product in a cart
/// </summary>
public class CartProductDto
{
    /// <summary>
    /// Product id
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Quantity of the product
    /// </summary>
    public int Quantity { get; set; }
}
