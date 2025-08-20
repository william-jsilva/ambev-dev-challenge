using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event that is raised when a sale is modified
/// </summary>
public class SaleModifiedEvent
{
    /// <summary>
    /// The sale that was modified
    /// </summary>
    public Sale Sale { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SaleModifiedEvent"/> class
    /// </summary>
    /// <param name="sale">The sale that was modified</param>
    public SaleModifiedEvent(Sale sale)
    {
        Sale = sale ?? throw new ArgumentNullException(nameof(sale));
    }
}
