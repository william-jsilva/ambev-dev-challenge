using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>
/// Handler for processing UpdateSaleCommand requests
/// </summary>
/// <param name="saleRepository">The sale repository</param>
/// <param name="mapper">The AutoMapper instance</param>
public class UpdateSaleHandler(ISaleRepository saleRepository, IMapper mapper)
    : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    /// <summary>
    /// Handles the UpdateSaleCommand request
    /// </summary>
    /// <param name="request">The UpdateSale command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created sale details</returns>
    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var existingSale = await saleRepository.GetByIdAsync(request.Id, cancellationToken) ??
            throw new KeyNotFoundException($"Sale with Id {request.Id} not found.");

        existingSale.UserId = request.UserId;
        existingSale.Date = request.Date;
        existingSale.UpdatedAt = DateTimeOffset.UtcNow;

        UpdateSaleProduct(request, existingSale);

        await saleRepository.UpdateAsync(existingSale, cancellationToken);

        var sale = await saleRepository.GetByIdAsync(request.Id, cancellationToken) ??
            throw new KeyNotFoundException($"Sale with ID {request.Id} not found");

        return mapper.Map<UpdateSaleResult>(sale);
    }

    /// <summary>
    /// Updates the products in the sale based on the incoming command.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="existingSale"></param>
    private static void UpdateSaleProduct(UpdateSaleCommand command, Sale existingSale)
    {
        var existingProducts = existingSale.Products.ToList();
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
                var newProduct = new SaleProduct
                {
                    SaleId = existingSale.Id,
                    ProductId = incoming.ProductId,
                    Quantity = incoming.Quantity
                };
                existingSale.Products.Add(newProduct);
            }
            else if (existing.Quantity != incoming.Quantity)
            {
                existing.Quantity = incoming.Quantity;
                existing.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }
    }
}
