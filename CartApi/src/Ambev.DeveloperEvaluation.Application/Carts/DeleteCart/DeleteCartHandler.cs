using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.DeleteCart;

/// <summary>
/// Handler for processing DeleteCartCommand requests
/// </summary>
public class DeleteCartHandler(ICartRepository cartRepository)
    : IRequestHandler<DeleteCartCommand, DeleteCartResponse>
{
    /// <summary>
    /// Handles the DeleteCartCommand request
    /// </summary>
    /// <param name="request">The DeleteCart command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the delete operation</returns>
    public async Task<DeleteCartResponse> Handle(DeleteCartCommand request, CancellationToken cancellationToken)
    {
        var validator = new DeleteCartValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var cart = await cartRepository.GetByIdAsync(request.Id, cancellationToken) ??
            throw new KeyNotFoundException($"Cart with ID {request.Id} not found");

        if (cart.IsCompleted())
            throw new InvalidOperationException($"Cart with ID {request.Id} is completed. Can't delete a completed cart.");

        cart.DefineDeleted();

        await cartRepository.UpdateAsync(cart, cancellationToken);

        return new DeleteCartResponse { Success = true };
    }
}
