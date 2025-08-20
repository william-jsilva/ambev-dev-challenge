namespace Ambev.DeveloperEvaluation.Application.Events;

/// <summary>
/// Interface for publishing domain events
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes a domain event asynchronously
    /// </summary>
    /// <param name="event">The domain event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task PublishAsync(object @event, CancellationToken cancellationToken = default);
}
