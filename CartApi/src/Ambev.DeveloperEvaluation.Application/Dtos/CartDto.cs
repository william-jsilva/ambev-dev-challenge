namespace Ambev.DeveloperEvaluation.Application.Dtos;

/// <summary>
/// Data Transfer Object for Cart
/// </summary>
public class CartDto
{
    /// <summary>
    /// Cart Id
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
    /// List of <see cref="CartProductDto "/>
    /// </summary>
    public List<CartProductDto> Products { get; set; } = [];
}