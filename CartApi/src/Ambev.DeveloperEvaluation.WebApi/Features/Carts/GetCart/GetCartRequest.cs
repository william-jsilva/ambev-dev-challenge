namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts.GetCart;

/// <summary>
/// Request model for getting a cart by ID
/// </summary>
public class GetCartRequest
{
    /// <summary>
    /// The unique identifier of the cart to retrieve
    /// </summary>
    public Guid Id { get; set; }
}
