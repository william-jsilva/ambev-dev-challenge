using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Events;

/// <summary>
/// Simple logging implementation for publishing domain events
/// This logs events instead of publishing to a message broker
/// </summary>
public class LoggingEventPublisher : IEventPublisher
{
    private readonly ILogger<LoggingEventPublisher> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingEventPublisher"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public LoggingEventPublisher(ILogger<LoggingEventPublisher> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task PublishAsync(object @event, CancellationToken cancellationToken = default)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        var eventType = @event.GetType().Name;
        _logger.LogInformation("Event published: {EventType} - {@Event}", eventType, @event);
        
        await Task.CompletedTask;
    }
}

