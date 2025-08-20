namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Data Transfer Object for Cart
/// </summary>
public class Cart
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
    /// List of <see cref="CartProduct "/>
    /// </summary>
    public List<CartProduct> Products { get; set; } = [];
}