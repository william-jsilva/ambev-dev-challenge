using Ambev.DeveloperEvaluation.Application.Dtos;
using Ambev.DeveloperEvaluation.Application.Products.Commands;
using Ambev.DeveloperEvaluation.Application.Products.Handlers;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using Bogus;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Products.Handlers;

public class CreateProductHandlerTests
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    private readonly CreateProductHandler _handler;
    private readonly Faker<ProductDto> _productDtoFaker;
    private readonly Faker<Product> _productFaker;

    public CreateProductHandlerTests()
    {
        _repository = Substitute.For<IProductRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new CreateProductHandler(_repository, _mapper);

        _productDtoFaker = new Faker<ProductDto>()
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
    }

    [Fact]
    public async Task Handle_ValidProduct_ShouldCreateAndReturnProduct()
    {
        // Arrange
        var productDto = _productDtoFaker.Generate();
        var command = new CreateProductCommand(productDto);
        var expectedProduct = _productFaker.Generate();
        var expectedProductDto = _productDtoFaker.Generate();

        _mapper.Map<Product>(productDto).Returns(expectedProduct);
        _repository.CreateAsync(expectedProduct, Arg.Any<CancellationToken>()).Returns(expectedProduct);
        _mapper.Map<ProductDto>(expectedProduct).Returns(expectedProductDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedProductDto);
        await _repository.Received(1).CreateAsync(expectedProduct, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var productDto = _productDtoFaker.Generate();
        var command = new CreateProductCommand(productDto);
        var expectedProduct = _productFaker.Generate();
        var expectedException = new InvalidOperationException("Database error");

        _mapper.Map<Product>(productDto).Returns(expectedProduct);
        _repository.CreateAsync(expectedProduct, Arg.Any<CancellationToken>()).Returns(Task.FromException<Product>(expectedException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Database error");
    }
}
