using Ambev.DeveloperEvaluation.Application.Products.Handlers;
using Ambev.DeveloperEvaluation.Application.Products.Queries;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Products.Handlers;

public class GetCategoriesHandlerTests
{
    private readonly IProductRepository _repository;
    private readonly GetCategoriesHandler _handler;

    public GetCategoriesHandlerTests()
    {
        _repository = Substitute.For<IProductRepository>();
        _handler = new GetCategoriesHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldReturnCategories()
    {
        // Arrange
        var query = new GetCategoriesQuery();
        var expectedCategories = new[] { "Electronics", "Clothing", "Books", "Home & Garden" };

        _repository.GetCategoriesAsync(Arg.Any<CancellationToken>()).Returns(expectedCategories);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedCategories);
        await _repository.Received(1).GetCategoriesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyCategories_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetCategoriesQuery();
        var emptyCategories = Enumerable.Empty<string>();

        _repository.GetCategoriesAsync(Arg.Any<CancellationToken>()).Returns(emptyCategories);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        await _repository.Received(1).GetCategoriesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var query = new GetCategoriesQuery();
        var expectedException = new InvalidOperationException("Database error");

        _repository.GetCategoriesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromException<IEnumerable<string>>(expectedException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(query, CancellationToken.None));
        exception.Message.Should().Be("Database error");
    }
}
