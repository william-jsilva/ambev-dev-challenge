using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event that is raised when a sale item (product) is cancelled
/// </summary>
public class ItemCancelledEvent
{
    /// <summary>
    /// The sale product that was cancelled
    /// </summary>
    public SaleProduct SaleProduct { get; }

    /// <summary>
    /// The sale that contains the cancelled product
    /// </summary>
    public Sale Sale { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemCancelledEvent"/> class
    /// </summary>
    /// <param name="saleProduct">The sale product that was cancelled</param>
    /// <param name="sale">The sale that contains the cancelled product</param>
    public ItemCancelledEvent(SaleProduct saleProduct, Sale sale)
    {
        SaleProduct = saleProduct ?? throw new ArgumentNullException(nameof(saleProduct));
        Sale = sale ?? throw new ArgumentNullException(nameof(sale));
    }
}

