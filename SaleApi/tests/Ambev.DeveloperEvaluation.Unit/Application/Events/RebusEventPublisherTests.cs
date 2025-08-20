using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Events;

/// <summary>
/// Tests for LoggingEventPublisher
/// </summary>
public class LoggingEventPublisherTests
{
    private readonly Mock<ILogger<LoggingEventPublisher>> _mockLogger;
    private readonly LoggingEventPublisher _publisher;

    public LoggingEventPublisherTests()
    {
        _mockLogger = new Mock<ILogger<LoggingEventPublisher>>();
        _publisher = new LoggingEventPublisher(_mockLogger.Object);
    }

    [Fact]
    public async Task PublishAsync_WithValidEvent_ShouldLogEvent()
    {
        // Arrange
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = 1,
            UserId = Guid.NewGuid(),
            Branch = "Test Branch",
            TotalSaleAmount = 100.00m
        };
        var @event = new SaleCreatedEvent(sale);

        // Act
        await _publisher.PublishAsync(@event);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Event published: SaleCreatedEvent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithNullEvent_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _publisher.PublishAsync(null!));
    }
}
