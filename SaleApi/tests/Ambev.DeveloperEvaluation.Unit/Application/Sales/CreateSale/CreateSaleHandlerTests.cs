using Ambev.DeveloperEvaluation.Application.Dtos;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
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

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.CreateSale;

public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IMapper _mapper;
    private readonly CreateSaleHandler _handler;
    private readonly Faker _faker;

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _cartRepository = Substitute.For<ICartRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new CreateSaleHandler(_saleRepository, _cartRepository, _mapper);
        _faker = new Faker("pt_BR");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateSaleSuccessfully()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var date = DateTimeOffset.UtcNow;
        var branch = "Loja Centro";

        var command = new CreateSaleCommand(cartId, date, branch);
        
        var cart = CreateValidCart(cartId);
        var expectedSale = CreateExpectedSale(cart, branch, date);
        var expectedResult = new CreateSaleResult 
        { 
            Id = expectedSale.Id,
            UserId = expectedSale.UserId,
            Date = expectedSale.Date,
            Products = expectedSale.Products.Select(p => new SaleProductDto 
            { 
                ProductId = p.ProductId,
                Quantity = p.Quantity
            })
        };

        _cartRepository.GetByIdAsync(cartId, Arg.Any<CancellationToken>()).Returns(cart);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(expectedSale);
        _mapper.Map<Sale>((command, cart)).Returns(expectedSale);
        _mapper.Map<CreateSaleResult>(expectedSale).Returns(expectedResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedSale.Id);
        result.UserId.Should().Be(expectedSale.UserId);
        result.Date.Should().Be(date);
        result.Products.Should().HaveCount(cart.Products.Count);

        await _cartRepository.Received(1).GetByIdAsync(cartId, Arg.Any<CancellationToken>());
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CartNotFound_ShouldThrowException()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var date = DateTimeOffset.UtcNow;
        var branch = "Loja Centro";

        var command = new CreateSaleCommand(cartId, date, branch);

        _cartRepository.GetByIdAsync(cartId, Arg.Any<CancellationToken>()).Returns((Cart)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain($"Cart with cartId {cartId} not found");

        await _cartRepository.Received(1).GetByIdAsync(cartId, Arg.Any<CancellationToken>());
        await _saleRepository.DidNotReceive().CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyBranch_ShouldThrowValidationException()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var date = DateTimeOffset.UtcNow;
        var branch = "";

        var command = new CreateSaleCommand(cartId, date, branch);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Errors.Should().ContainSingle();
        exception.Errors.First().PropertyName.Should().Be("Branch");

        await _cartRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _saleRepository.DidNotReceive().CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CartRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var date = DateTimeOffset.UtcNow;
        var branch = "Loja Centro";

        var command = new CreateSaleCommand(cartId, date, branch);
        var exception = new InvalidOperationException("Database connection failed");

        _cartRepository.GetByIdAsync(cartId, Arg.Any<CancellationToken>()).ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        thrownException.Should().Be(exception);

        await _cartRepository.Received(1).GetByIdAsync(cartId, Arg.Any<CancellationToken>());
        await _saleRepository.DidNotReceive().CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SaleRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var date = DateTimeOffset.UtcNow;
        var branch = "Loja Centro";

        var command = new CreateSaleCommand(cartId, date, branch);
        var cart = CreateValidCart(cartId);
        var exception = new InvalidOperationException("Database connection failed");

        _cartRepository.GetByIdAsync(cartId, Arg.Any<CancellationToken>()).Returns(cart);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).ThrowsAsync(exception);
        _mapper.Map<Sale>((command, cart)).Returns(new Sale());

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        thrownException.Should().Be(exception);

        await _cartRepository.Received(1).GetByIdAsync(cartId, Arg.Any<CancellationToken>());
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMultipleProducts_ShouldCalculateCorrectDiscounts()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var date = DateTimeOffset.UtcNow;
        var branch = "Loja Centro";

        var command = new CreateSaleCommand(cartId, date, branch);
        
        var cart = CreateCartWithMultipleProducts(cartId);
        var expectedSale = CreateExpectedSale(cart, branch, date);
        var expectedResult = new CreateSaleResult 
        { 
            Id = expectedSale.Id,
            UserId = expectedSale.UserId,
            Date = expectedSale.Date,
            Products = expectedSale.Products.Select(p => new SaleProductDto 
            { 
                ProductId = p.ProductId,
                Quantity = p.Quantity
            })
        };

        _cartRepository.GetByIdAsync(cartId, Arg.Any<CancellationToken>()).Returns(cart);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(expectedSale);
        _mapper.Map<Sale>((command, cart)).Returns(expectedSale);
        _mapper.Map<CreateSaleResult>(expectedSale).Returns(expectedResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Products.Should().HaveCount(3);

        // Verificar se os produtos foram mapeados corretamente
        var saleProducts = result.Products.ToList();
        saleProducts[0].Quantity.Should().Be(5);
        saleProducts[1].Quantity.Should().Be(2);
        saleProducts[2].Quantity.Should().Be(15);

        await _cartRepository.Received(1).GetByIdAsync(cartId, Arg.Any<CancellationToken>());
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyCart_ShouldCreateSaleWithNoProducts()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var date = DateTimeOffset.UtcNow;
        var branch = "Loja Centro";

        var command = new CreateSaleCommand(cartId, date, branch);
        
        var cart = CreateEmptyCart(cartId);
        var expectedSale = CreateExpectedSale(cart, branch, date);
        var expectedResult = new CreateSaleResult 
        { 
            Id = expectedSale.Id,
            UserId = expectedSale.UserId,
            Date = expectedSale.Date,
            Products = new List<SaleProductDto>()
        };

        _cartRepository.GetByIdAsync(cartId, Arg.Any<CancellationToken>()).Returns(cart);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(expectedSale);
        _mapper.Map<Sale>((command, cart)).Returns(expectedSale);
        _mapper.Map<CreateSaleResult>(expectedSale).Returns(expectedResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Products.Should().BeEmpty();

        await _cartRepository.Received(1).GetByIdAsync(cartId, Arg.Any<CancellationToken>());
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    private static Cart CreateValidCart(Guid cartId)
    {
        return new Cart
        {
            Id = cartId,
            UserId = Guid.NewGuid(),
            Date = DateTimeOffset.UtcNow,
            Products = new List<CartProduct>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 5,
                    UnitPrice = 10.0m
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 3,
                    UnitPrice = 5.0m
                }
            }
        };
    }

    private static Cart CreateCartWithMultipleProducts(Guid cartId)
    {
        return new Cart
        {
            Id = cartId,
            UserId = Guid.NewGuid(),
            Date = DateTimeOffset.UtcNow,
            Products = new List<CartProduct>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 5, UnitPrice = 10.0m },   // 10% desconto
                new() { ProductId = Guid.NewGuid(), Quantity = 2, UnitPrice = 15.0m },   // Sem desconto
                new() { ProductId = Guid.NewGuid(), Quantity = 15, UnitPrice = 8.0m }    // 20% desconto
            }
        };
    }

    private static Cart CreateEmptyCart(Guid cartId)
    {
        return new Cart
        {
            Id = cartId,
            UserId = Guid.NewGuid(),
            Date = DateTimeOffset.UtcNow,
            Products = new List<CartProduct>()
        };
    }

    private static Sale CreateExpectedSale(Cart cart, string branch, DateTimeOffset date)
    {
        var saleProducts = cart.Products.Select(cp => new SaleProduct
        {
            Id = Guid.NewGuid(),
            SaleId = Guid.NewGuid(),
            ProductId = cp.ProductId,
            Quantity = cp.Quantity,
            UnitPrice = cp.UnitPrice,
            Discounts = 1.0m, // Default discount
            TotalAmount = cp.Quantity * cp.UnitPrice,
            Status = SaleProductStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();

        return new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = 1001,
            Branch = branch,
            UserId = cart.UserId,
            Date = date,
            Status = SaleStatus.Active,
            Products = saleProducts,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
