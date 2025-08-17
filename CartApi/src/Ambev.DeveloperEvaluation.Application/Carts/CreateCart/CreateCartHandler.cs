using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.CreateCart;

/// <summary>
/// Handler for processing CreateCartCommand requests
/// </summary>
/// <param name="cartRepository">The cart repository</param>
/// <param name="mapper">The AutoMapper instance</param>
public class CreateCartHandler(ICartRepository cartRepository, IMapper mapper) : IRequestHandler<CreateCartCommand, CreateCartResult>
{
    /// <summary>
    /// Handles the CreateCartCommand request
    /// </summary>
    /// <param name="command">The CreateCart command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created cart details</returns>
    public async Task<CreateCartResult> Handle(CreateCartCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateCartCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var existingCart = await cartRepository.GetActiveByUserIdAsync(command.UserId, cancellationToken);
        if (existingCart != null)
            throw new InvalidOperationException($"Cart with userId {command.UserId} already exists");

        var cart = mapper.Map<Cart>(command);

        var createdCart = await cartRepository.CreateAsync(cart, cancellationToken);
        var result = mapper.Map<CreateCartResult>(createdCart);

        return result;
    }
}
