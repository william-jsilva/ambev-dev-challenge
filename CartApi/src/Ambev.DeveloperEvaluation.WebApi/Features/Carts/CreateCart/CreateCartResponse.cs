using Ambev.DeveloperEvaluation.Application.Dtos;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts.CreateCart;

/// <summary>
/// API response model for CreateCart operation
/// </summary>
public class CreateCartResponse
{
    /// <summary>
    /// The unique identifier of the created cart
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User Id
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Date of cart
    /// </summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>
    /// List of produts
    /// </summary>
    public IEnumerable<CartProductDto> Products { get; set; } = [];
}