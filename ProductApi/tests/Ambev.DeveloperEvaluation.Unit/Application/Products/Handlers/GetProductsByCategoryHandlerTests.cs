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

public class GetProductsByCategoryHandlerTests
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    private readonly GetProductsByCategoryHandler _handler;
    private readonly Faker<Product> _productFaker;
    private readonly Faker<ProductDto> _productDtoFaker;

    public GetProductsByCategoryHandlerTests()
    {
        _repository = Substitute.For<IProductRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetProductsByCategoryHandler(_repository, _mapper);

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
    public async Task Handle_ValidCategory_ShouldReturnProductsInCategory()
    {
        // Arrange
        var category = "Electronics";
        var queryParams = new QueryParameters { Page = 1, Size = 10 };
        var query = new GetProductsByCategoryQuery(category, queryParams);
        var products = _productFaker.Generate(3);
        var productDtos = _productDtoFaker.Generate(3);
        var totalItems = 15;

        _repository.GetAsync(Arg.Any<QueryParameters>(), Arg.Any<CancellationToken>()).Returns((products, totalItems));
        _mapper.Map<IEnumerable<ProductDto>>(products).Returns(productDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(3);
        result.TotalItems.Should().Be(totalItems);
        result.CurrentPage.Should().Be(1);
        result.TotalPages.Should().Be(2); // 15 items / 10 per page = 2 pages
        
        // Verify that category filter was added to query parameters
        await _repository.Received(1).GetAsync(Arg.Is<QueryParameters>(qp => 
            qp.Filters.ContainsKey("Category") && qp.Filters["Category"] == category), 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyCategory_ShouldReturnEmptyResult()
    {
        // Arrange
        var category = "";
        var queryParams = new QueryParameters { Page = 1, Size = 10 };
        var query = new GetProductsByCategoryQuery(category, queryParams);
        var emptyProducts = Enumerable.Empty<Product>();
        var emptyProductDtos = Enumerable.Empty<ProductDto>();
        var totalItems = 0;

        _repository.GetAsync(Arg.Any<QueryParameters>(), Arg.Any<CancellationToken>()).Returns((emptyProducts, totalItems));
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
    public async Task Handle_WithAdditionalFilters_ShouldPreserveFilters()
    {
        // Arrange
        var category = "Electronics";
        var queryParams = new QueryParameters 
        { 
            Page = 1, 
            Size = 10,
            Filters = new Dictionary<string, string>
            {
                { "Price_min", "100" },
                { "Price_max", "500" }
            }
        };
        var query = new GetProductsByCategoryQuery(category, queryParams);
        var products = _productFaker.Generate(2);
        var productDtos = _productDtoFaker.Generate(2);
        var totalItems = 2;

        _repository.GetAsync(Arg.Any<QueryParameters>(), Arg.Any<CancellationToken>()).Returns((products, totalItems));
        _mapper.Map<IEnumerable<ProductDto>>(products).Returns(productDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        
        // Verify that both category and existing filters are preserved
        await _repository.Received(1).GetAsync(Arg.Is<QueryParameters>(qp => 
            qp.Filters.ContainsKey("Category") && 
            qp.Filters["Category"] == category &&
            qp.Filters.ContainsKey("Price_min") &&
            qp.Filters.ContainsKey("Price_max")), 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var category = "Electronics";
        var queryParams = new QueryParameters { Page = 1, Size = 10 };
        var query = new GetProductsByCategoryQuery(category, queryParams);
        var expectedException = new InvalidOperationException("Database error");

        _repository.GetAsync(Arg.Any<QueryParameters>(), Arg.Any<CancellationToken>()).Returns(Task.FromException<(IEnumerable<Product>, int)>(expectedException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(query, CancellationToken.None));
        exception.Message.Should().Be("Database error");
    }
}
