using Ambev.DeveloperEvaluation.Application.Products.Commands;
using Ambev.DeveloperEvaluation.Application.Products.Handlers;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Products.Handlers;

public class DeleteProductHandlerTests
{
    private readonly IProductRepository _repository;
    private readonly DeleteProductHandler _handler;

    public DeleteProductHandlerTests()
    {
        _repository = Substitute.For<IProductRepository>();
        _handler = new DeleteProductHandler(_repository);
    }

    [Fact]
    public async Task Handle_ValidProductId_ShouldDeleteProduct()
    {
        // Arrange
        var productId = "test-id-123";
        var command = new DeleteProductCommand(productId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).DeleteAsync(productId, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_InvalidId_ShouldStillCallRepository(string invalidId)
    {
        // Arrange
        var command = new DeleteProductCommand(invalidId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).DeleteAsync(invalidId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var productId = "test-id-123";
        var command = new DeleteProductCommand(productId);
        var expectedException = new InvalidOperationException("Database error");

        _repository.DeleteAsync(productId, Arg.Any<CancellationToken>()).Returns(Task.FromException(expectedException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Database error");
    }
}
