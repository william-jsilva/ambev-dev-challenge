using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.GetCart;

/// <summary>
/// Handler for processing GetCartCommand requests
/// </summary>
/// <param name="cartRepository">The cart repository</param>
/// <param name="mapper">The AutoMapper instance</param>
public class GetCartHandler(ICartRepository cartRepository, IMapper mapper)
    : IRequestHandler<GetCartCommand, GetCartResult>
{
    /// <summary>
    /// Handles the GetCartCommand request
    /// </summary>
    /// <param name="request">The GetCart command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cart details if found</returns>
    public async Task<GetCartResult> Handle(GetCartCommand request, CancellationToken cancellationToken)
    {
        var validator = new GetCartValidator(); // ToDo: avaliar como não depender de uma instância específica (SOLID) sem quebrar a estrutura da Ambev [Interface?]
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var cart = await cartRepository.GetByIdAsync(request.Id, cancellationToken) ??
            throw new KeyNotFoundException($"Cart with ID {request.Id} not found");

        return mapper.Map<GetCartResult>(cart);
    }
}
