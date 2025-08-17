using Ambev.DeveloperEvaluation.Application.Dtos;

namespace Ambev.DeveloperEvaluation.Application.Carts.CreateCart;

/// <summary>
/// Represents the response returned after successfully creating a new cart.
/// </summary>
/// <remarks>
/// This response contains the unique identifier of the newly created cart,
/// which can be used for subsequent operations or reference.
/// </remarks>
public struct CreateCartResult()
{
    /// <summary>
    /// Gets or sets the unique identifier of the newly created cart.
    /// </summary>
    /// <value>A identifier the created cart in the system.</value>
    public Guid Id { get; set; } = default;

    /// <summary>
    /// User Id
    /// </summary>
    public Guid UserId { get; set; } = default;

    /// <summary>
    /// Date of cart
    /// </summary>
    public DateTimeOffset Date { get; set; } = default;

    /// <summary>
    /// List of <see cref="CartProductDto" />
    /// </summary>
    public IEnumerable<CartProductDto> Products { get; set; } = [];
}