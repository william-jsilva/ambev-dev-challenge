using Ambev.DeveloperEvaluation.Application.Dtos;
using Ambev.DeveloperEvaluation.Application.Products.Handlers;
using Ambev.DeveloperEvaluation.Application.Products.Queries;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using Bogus;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Products.Handlers;

public class GetProductByIdHandlerTests
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    private readonly GetProductByIdHandler _handler;
    private readonly Faker<Product> _productFaker;
    private readonly Faker<ProductDto> _productDtoFaker;

    public GetProductByIdHandlerTests()
    {
        _repository = Substitute.For<IProductRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetProductByIdHandler(_repository, _mapper);

        _productFaker = new Faker<Product>()
            .RuleFor(p => p.Id, f => f.Random.Guid().ToString())
            .RuleFor(p => p.Title, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => f.Random.Double(1, 1000))
            .RuleFor(p => p.Description, f => f.Lorem.Sentence())
            .RuleFor(p => p.Category, f => f.Commerce.Categories(1)[0])
            .RuleFor(p => p.Image, f => f.Image.PicsumUrl())
            .RuleFor(p => p.Rating, f => new Rating
            {
                Rate = f.Random.Double(0, 5),
                Count = f.Random.Int(0, 1000)
            });

        _productDtoFaker = new Faker<ProductDto>()
            .RuleFor(p => p.Id, f => f.Random.Guid().ToString())
            .RuleFor(p => p.Title, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => f.Random.Double(1, 1000))
            .RuleFor(p => p.Description, f => f.Lorem.Sentence())
            .RuleFor(p => p.Category, f => f.Commerce.Categories(1)[0])
            .RuleFor(p => p.Image, f => f.Image.PicsumUrl())
            .RuleFor(p => p.Rating, f => new RatingDto
            {
                Rate = f.Random.Double(0, 5),
                Count = f.Random.Int(0, 1000)
            });
    }

    [Fact]
    public async Task Handle_ExistingProduct_ShouldReturnProduct()
    {
        // Arrange
        var productId = "test-id-123";
        var query = new GetProductByIdQuery(productId);
        var product = _productFaker.Generate();
        var productDto = _productDtoFaker.Generate();

        _repository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);
        _mapper.Map<ProductDto>(product).Returns(productDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(productDto);
        await _repository.Received(1).GetByIdAsync(productId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonExistingProduct_ShouldReturnNull()
    {
        // Arrange
        var productId = "non-existing-id";
        var query = new GetProductByIdQuery(productId);

        _repository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns((Product?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        await _repository.Received(1).GetByIdAsync(productId, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_InvalidId_ShouldReturnNull(string invalidId)
    {
        // Arrange
        var query = new GetProductByIdQuery(invalidId);

        _repository.GetByIdAsync(invalidId, Arg.Any<CancellationToken>()).Returns((Product?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        await _repository.Received(1).GetByIdAsync(invalidId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var productId = "test-id-123";
        var query = new GetProductByIdQuery(productId);
        var expectedException = new InvalidOperationException("Database error");

        _repository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(Task.FromException<Product?>(expectedException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(query, CancellationToken.None));
        exception.Message.Should().Be("Database error");
    }
}
