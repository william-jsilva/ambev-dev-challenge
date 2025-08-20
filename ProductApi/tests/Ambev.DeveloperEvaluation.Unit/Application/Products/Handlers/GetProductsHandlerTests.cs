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

public class GetProductsHandlerTests
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    private readonly GetProductsHandler _handler;
    private readonly Faker<Product> _productFaker;
    private readonly Faker<ProductDto> _productDtoFaker;

    public GetProductsHandlerTests()
    {
        _repository = Substitute.For<IProductRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetProductsHandler(_repository, _mapper);

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
    public async Task Handle_ValidQuery_ShouldReturnPaginationResult()
    {
        // Arrange
        var query = new GetProductsQuery(new QueryParameters { Page = 1, Size = 10 });
        var products = _productFaker.Generate(5);
        var productDtos = _productDtoFaker.Generate(5);
        var totalItems = 25;

        _repository.GetAsync(query.Query, Arg.Any<CancellationToken>()).Returns((products, totalItems));
        _mapper.Map<IEnumerable<ProductDto>>(products).Returns(productDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(5);
        result.TotalItems.Should().Be(totalItems);
        result.CurrentPage.Should().Be(1);
        result.TotalPages.Should().Be(3); // 25 items / 10 per page = 3 pages
        await _repository.Received(1).GetAsync(query.Query, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyResult_ShouldReturnEmptyPaginationResult()
    {
        // Arrange
        var query = new GetProductsQuery(new QueryParameters { Page = 1, Size = 10 });
        var emptyProducts = Enumerable.Empty<Product>();
        var emptyProductDtos = Enumerable.Empty<ProductDto>();
        var totalItems = 0;

        _repository.GetAsync(query.Query, Arg.Any<CancellationToken>()).Returns((emptyProducts, totalItems));
        _mapper.Map<IEnumerable<ProductDto>>(emptyProducts).Returns(emptyProductDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEmpty();
        result.TotalItems.Should().Be(0);
        result.CurrentPage.Should().Be(1);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithFilters_ShouldPassFiltersToRepository()
    {
        // Arrange
        var queryParams = new QueryParameters 
        { 
            Page = 1, 
            Size = 10,
            Filters = new Dictionary<string, string>
            {
                { "Category", "Electronics" },
                { "Price_min", "100" },
                { "Price_max", "500" }
            }
        };
        var query = new GetProductsQuery(queryParams);
        var products = _productFaker.Generate(3);
        var productDtos = _productDtoFaker.Generate(3);
        var totalItems = 3;

        _repository.GetAsync(queryParams, Arg.Any<CancellationToken>()).Returns((products, totalItems));
        _mapper.Map<IEnumerable<ProductDto>>(products).Returns(productDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(3);
        await _repository.Received(1).GetAsync(queryParams, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var query = new GetProductsQuery(new QueryParameters { Page = 1, Size = 10 });
        var expectedException = new InvalidOperationException("Database error");

        _repository.GetAsync(query.Query, Arg.Any<CancellationToken>()).Returns(Task.FromException<(IEnumerable<Product>, int)>(expectedException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(query, CancellationToken.None));
        exception.Message.Should().Be("Database error");
    }
}
