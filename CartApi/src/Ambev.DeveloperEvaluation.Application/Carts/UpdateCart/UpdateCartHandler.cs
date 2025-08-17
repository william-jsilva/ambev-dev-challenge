using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;

/// <summary>
/// Handler for processing UpdateCartCommand requests
/// </summary>
/// <param name="cartRepository">The cart repository</param>
/// <param name="mapper">The AutoMapper instance</param>
public class UpdateCartHandler(ICartRepository cartRepository, IMapper mapper)
    : IRequestHandler<UpdateCartCommand, UpdateCartResult>
{
    /// <summary>
    /// Handles the UpdateCartCommand request
    /// </summary>
    /// <param name="request">The UpdateCart command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created cart details</returns>
    public async Task<UpdateCartResult> Handle(UpdateCartCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateCartCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var existingCart = await cartRepository.GetByIdAsync(request.Id, cancellationToken) ??
            throw new KeyNotFoundException($"Cart with Id {request.Id} not found.");

        existingCart.UserId = request.UserId;
        existingCart.Date = request.Date;
        existingCart.UpdatedAt = DateTimeOffset.UtcNow;

        UpdateCartProduct(request, existingCart);

        await cartRepository.UpdateAsync(existingCart, cancellationToken);

        var cart = await cartRepository.GetByIdAsync(request.Id, cancellationToken) ??
            throw new KeyNotFoundException($"Cart with ID {request.Id} not found");

        return mapper.Map<UpdateCartResult>(cart);
    }

    /// <summary>
    /// Updates the products in the cart based on the incoming command.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="existingCart"></param>
    private static void UpdateCartProduct(UpdateCartCommand command, Cart existingCart)
    {
        var existingProducts = existingCart.Products.ToList();
        var incomingProducts = command.Products.ToList();

        foreach (var existing in existingProducts)
        {
            if (!incomingProducts.Any(product => product.ProductId == existing.ProductId))
                existing.DefineDeleted();

        }

        foreach (var incoming in incomingProducts)
        {
            var existing = existingProducts.FirstOrDefault(product => product.ProductId == incoming.ProductId);

            if (existing == null)
            {
                var newProduct = new CartProduct
                {
                    CartId = existingCart.Id,
                    ProductId = incoming.ProductId,
                    Quantity = incoming.Quantity
                };
                existingCart.Products.Add(newProduct);
            }
            else if (existing.Quantity != incoming.Quantity)
            {
                existing.Quantity = incoming.Quantity;
                existing.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }
    }
}
