using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents product in sale with its quantity.
/// </summary>
public class SaleProduct : BaseEntity // ToDo: avaliar de não usar class e não usar BaseEntity sem afetar o Entity Framework
{
    /// <inheritdoc />
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public SaleProduct()
    {
        CreatedAt = DateTimeOffset.UtcNow;
        Status = SaleProductStatus.Active;
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// Sale id
    /// </summary>
    public Guid SaleId { get; set; }

    /// <summary>
    /// Product id
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Quantity of the product
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Unit price of the product
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Total price of the product (Quantity * UnitPrice)
    /// </summary>
    public decimal Discounts { get; set; }

    /// <summary>
    /// Total amount of the product (Quantity * UnitPrice - Discounts)
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// The current status of the product's sale
    /// </summary>
    public SaleProductStatus Status { get; set; }

    /// <summary>
    /// Gets the date and time when the user was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets the date and time of the last update to the sale's information.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Gets the date and time of deleted sale
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Sale associated with this product (navigation property)
    /// </summary>
    public Sale Sale { get; set; }

    public void DefineDeleted()
    {
        Status = SaleProductStatus.Deleted;
        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
};
