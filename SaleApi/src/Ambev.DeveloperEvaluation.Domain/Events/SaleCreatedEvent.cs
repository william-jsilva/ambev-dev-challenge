using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event that is raised when a sale is created
/// </summary>
public class SaleCreatedEvent
{
    /// <summary>
    /// The sale that was created
    /// </summary>
    public Sale Sale { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SaleCreatedEvent"/> class
    /// </summary>
    /// <param name="sale">The sale that was created</param>
    public SaleCreatedEvent(Sale sale)
    {
        Sale = sale ?? throw new ArgumentNullException(nameof(sale));
    }
}

