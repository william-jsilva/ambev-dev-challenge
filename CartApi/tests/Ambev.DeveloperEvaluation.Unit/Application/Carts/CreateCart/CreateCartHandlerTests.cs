using Ambev.DeveloperEvaluation.Application.Carts.CreateCart;
using Ambev.DeveloperEvaluation.Application.Dtos;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using Bogus;
using FluentAssertions;
using MediatR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Carts.CreateCart;

public class CreateCartHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly IMapper _mapper;
    private readonly CreateCartHandler _handler;
    private readonly Faker _faker;

    public CreateCartHandlerTests()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new CreateCartHandler(_cartRepository, _mapper);
        _faker = new Faker("pt_BR");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateCartSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateTimeOffset.UtcNow;
        var products = CreateValidCartProducts(2);

        var command = new CreateCartCommand(userId, date, products);
        var expectedCart = CreateExpectedCart(userId, date, products);
        var expectedResult = new CreateCartResult 
        { 
            Id = expectedCart.Id,
            UserId = expectedCart.UserId,
            Date = expectedCart.Date,
            Products = expectedCart.Products.Select(p => new CartProductDto 
            { 
                ProductId = p.ProductId,
                Quantity = p.Quantity
            })
        };

        _cartRepository.GetActiveByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns((Cart)null);
        _cartRepository.CreateAsync(Arg.Any<Cart>(), Arg.Any<CancellationToken>()).Returns(expectedCart);
        _mapper.Map<Cart>(command).Returns(expectedCart);
        _mapper.Map<CreateCartResult>(expectedCart).Returns(expectedResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedCart.Id);
        result.UserId.Should().Be(userId);
        result.Date.Should().Be(date);
        result.Products.Should().HaveCount(2);

        await _cartRepository.Received(1).CreateAsync(Arg.Any<Cart>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyProductsList_ShouldThrowValidationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateTimeOffset.UtcNow;
        var products = new List<CartProduct>();

        var command = new CreateCartCommand(userId, date, products);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Errors.Should().ContainSingle();
        exception.Errors.First().PropertyName.Should().Be("Products");

        await _cartRepository.DidNotReceive().CreateAsync(Arg.Any<Cart>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InvalidUserId_ShouldThrowValidationException()
    {
        // Arrange
        var userId = Guid.Empty;
        var date = DateTimeOffset.UtcNow;
        var products = CreateValidCartProducts(1);

        var command = new CreateCartCommand(userId, date, products);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Errors.Should().ContainSingle();
        exception.Errors.First().PropertyName.Should().Be("UserId");

        await _cartRepository.DidNotReceive().CreateAsync(Arg.Any<Cart>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingCartForUser_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateTimeOffset.UtcNow;
        var products = CreateValidCartProducts(1);

        var command = new CreateCartCommand(userId, date, products);
        var existingCart = CreateExpectedCart(userId, date, products);

        _cartRepository.GetActiveByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(existingCart);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain($"Cart with userId {userId} already exists");

        await _cartRepository.DidNotReceive().CreateAsync(Arg.Any<Cart>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateTimeOffset.UtcNow;
        var products = CreateValidCartProducts(1);

        var command = new CreateCartCommand(userId, date, products);
        var exception = new InvalidOperationException("Database connection failed");

        _cartRepository.GetActiveByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns((Cart)null);
        _cartRepository.CreateAsync(Arg.Any<Cart>(), Arg.Any<CancellationToken>()).ThrowsAsync(exception);
        _mapper.Map<Cart>(command).Returns(new Cart());

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        thrownException.Should().Be(exception);

        await _cartRepository.Received(1).CreateAsync(Arg.Any<Cart>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MultipleProducts_ShouldCreateCartWithAllProducts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateTimeOffset.UtcNow;
        var products = CreateValidCartProducts(5);

        var command = new CreateCartCommand(userId, date, products);
        var expectedCart = CreateExpectedCart(userId, date, products);
        var expectedResult = new CreateCartResult 
        { 
            Id = expectedCart.Id,
            UserId = expectedCart.UserId,
            Date = expectedCart.Date,
            Products = expectedCart.Products.Select(p => new CartProductDto 
            { 
                ProductId = p.ProductId,
                Quantity = p.Quantity
            })
        };

        _cartRepository.GetActiveByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns((Cart)null);
        _cartRepository.CreateAsync(Arg.Any<Cart>(), Arg.Any<CancellationToken>()).Returns(expectedCart);
        _mapper.Map<Cart>(command).Returns(expectedCart);
        _mapper.Map<CreateCartResult>(expectedCart).Returns(expectedResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Products.Should().HaveCount(5);

        await _cartRepository.Received(1).CreateAsync(Arg.Any<Cart>(), Arg.Any<CancellationToken>());
    }

    private static List<CartProduct> CreateValidCartProducts(int count)
    {
        var faker = new Faker();
        var products = new List<CartProduct>();

        for (int i = 0; i < count; i++)
        {
            products.Add(new CartProduct
            {
                Id = Guid.NewGuid(),
                CartId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = faker.Random.Int(1, 10),
                Status = CartProductStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        return products;
    }

    private static Cart CreateExpectedCart(Guid userId, DateTimeOffset date, List<CartProduct> products)
    {
        return new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Date = date,
            Status = CartStatus.Active,
            Products = products,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
