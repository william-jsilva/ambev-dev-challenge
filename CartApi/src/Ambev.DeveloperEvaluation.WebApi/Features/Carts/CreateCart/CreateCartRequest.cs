using Ambev.DeveloperEvaluation.Application.Dtos;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts.CreateCart;

/// <summary>
/// Represents a request to create a new cart in the system.
/// </summary>
/// <param name="UserId">User Id</param>
/// <param name="Date">Date of cart</param>
/// <param name="Products">List of produts</param>
public record CreateCartRequest
(
    Guid UserId,
    DateTimeOffset Date,
    IEnumerable<CartProductDto> Products
);