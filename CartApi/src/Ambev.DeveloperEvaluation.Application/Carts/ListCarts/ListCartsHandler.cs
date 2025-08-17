using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.ListCarts;

/// <summary>
/// Handler for processing ListCartsCommand requests
/// </summary>
/// <param name="cartRepository">The cart repository</param>
public class ListCartsHandler(ICartRepository cartRepository) : IRequestHandler<ListCartsCommand, ListCartsResult>
{
    /// <summary>
    /// Handles the ListCartsCommand request
    /// </summary>
    /// <param name="request">The ListCarts command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>page with carts</returns>
    public async Task<ListCartsResult> Handle(ListCartsCommand request, CancellationToken cancellationToken)
    {
        var validator = new ListCartsValidator(); // ToDo: avaliar como não depender de uma instância específica (SOLID) sem quebrar a estrutura da Ambev [Interface?]
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var carts = await cartRepository.GetAsync(request.Page, request.Size, request.Order, cancellationToken) ?? [];
        var totalItems = await cartRepository.GetTotalAsync(cancellationToken);

        return new ListCartsResult
        {
            Carts = carts,
            TotalItems = totalItems
        };
    }
}
