using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Validation;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Handler for processing CreateSaleCommand requests
/// </summary>
/// <param name="saleRepository">The sale repository</param>
/// <param name="cartRepository">The cart repository</param>
/// <param name="mapper">The AutoMapper instance</param>
/// <param name="eventPublisher">The event publisher</param>
public class CreateSaleHandler(ISaleRepository saleRepository, ICartRepository cartRepository, IMapper mapper, IEventPublisher eventPublisher) 
    : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    /// <summary>
    /// Handles the CreateSaleCommand request
    /// </summary>
    /// <param name="command">The CreateSale command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created sale details</returns>
    public async Task<CreateSaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        await ValidateCommand(command, cancellationToken);

        var cart = await cartRepository.GetByIdAsync(command.CartId, cancellationToken) ??
            throw new InvalidOperationException($"Cart with cartId {command.CartId} not found");

        var sale = mapper.Map<Sale>((command, cart));

        sale.CalculateTotalSaleAmount();

        await ValidateSale(sale, cancellationToken);

        var createdSale = await saleRepository.CreateAsync(sale, cancellationToken);

        // Publish SaleCreated event
        await eventPublisher.PublishAsync(new SaleCreatedEvent(createdSale), cancellationToken);

        var result = mapper.Map<CreateSaleResult>(createdSale);

        return result;
    }

    private static async Task ValidateCommand(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var createSaleCommandValidator = new CreateSaleCommandValidator();
        var createSaleCommandValidationResult = await createSaleCommandValidator.ValidateAsync(command, cancellationToken);

        if (!createSaleCommandValidationResult.IsValid)
            throw new ValidationException(createSaleCommandValidationResult.Errors);
    }

    private static async Task ValidateSale(Sale sale, CancellationToken cancellationToken)
    {
        var saleValidator = new SaleValidator();
        var saleValidatorResult = await saleValidator.ValidateAsync(sale, cancellationToken);

        if (!saleValidatorResult.IsValid)
            throw new ValidationException(saleValidatorResult.Errors);
    }
}
