using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts.GetCart;

/// <summary>
/// API response model for GetCart operation
/// </summary>
public class GetCartResponse
{
    /// <summary>
    /// The unique identifier of the cart
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The user identifier
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The current status of the cart
    /// </summary>
    public CartStatus Status { get; set; }
}
