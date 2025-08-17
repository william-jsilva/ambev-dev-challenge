using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents product in cart with its quantity.
/// </summary>
public class CartProduct : BaseEntity // ToDo: avaliar de não usar class e não usar BaseEntity sem afetar o Entity Framework
{
    /// <inheritdoc />
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public CartProduct()
    {
        CreatedAt = DateTimeOffset.UtcNow;
        Status = CartProductStatus.Active;
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// Cart id
    /// </summary>
    public Guid CartId { get; set; }

    /// <summary>
    /// Product id
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Quantity of the product
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// The current status of the product's cart
    /// </summary>
    public CartProductStatus Status { get; set; }

    /// <summary>
    /// Gets the date and time when the user was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets the date and time of the last update to the cart's information.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Gets the date and time of deleted cart
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Cart associated with this product (navigation property)
    /// </summary>
    public Cart Cart { get; set; }

    public void DefineDeleted()
    {
        Status = CartProductStatus.Deleted;
        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
};
