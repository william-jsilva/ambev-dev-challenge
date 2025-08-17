using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts.UpdateCart;

/// <summary>
/// API response model for UpdateCart operation
/// </summary>
/// <param name="Id">The unique identifier of the cart</param>
/// <param name="UserId">User Id</param>
/// <param name="Date">Date of cart</param>
/// <param name="Products">List of produts</param>
public record UpdateCartResponse
(
    Guid Id,
    Guid UserId,
    DateTimeOffset Date,
    IEnumerable<CartProduct> Products
);