using Ambev.DeveloperEvaluation.Application.Dtos;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts.UpdateCart;

/// <summary>
/// Represents a request to update a cart in the system.
/// </summary>
/// <param name="UserId">User Id</param>
/// <param name="Date">Date of cart</param>
/// <param name="Products">List of produts</param>
public record UpdateCartRequest
(
    Guid UserId,
    DateTimeOffset Date,
    IEnumerable<CartProductDto> Products
);