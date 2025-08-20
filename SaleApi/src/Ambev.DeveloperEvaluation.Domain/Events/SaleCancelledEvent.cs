using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event that is raised when a sale is cancelled
/// </summary>
public class SaleCancelledEvent
{
    /// <summary>
    /// The sale that was cancelled
    /// </summary>
    public Sale Sale { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SaleCancelledEvent"/> class
    /// </summary>
    /// <param name="sale">The sale that was cancelled</param>
    public SaleCancelledEvent(Sale sale)
    {
        Sale = sale ?? throw new ArgumentNullException(nameof(sale));
    }
}
