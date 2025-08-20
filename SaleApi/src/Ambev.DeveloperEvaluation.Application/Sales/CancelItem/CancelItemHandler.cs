using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelItem;

/// <summary>
/// Handler for processing CancelItemCommand requests
/// </summary>
/// <param name="saleRepository">The sale repository</param>
/// <param name="eventPublisher">The event publisher</param>
public class CancelItemHandler(ISaleRepository saleRepository, IEventPublisher eventPublisher)
    : IRequestHandler<CancelItemCommand, CancelItemResult>
{
    /// <summary>
    /// Handles the CancelItemCommand request
    /// </summary>
    /// <param name="command">The CancelItem command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the cancel operation</returns>
    public async Task<CancelItemResult> Handle(CancelItemCommand command, CancellationToken cancellationToken)
    {
        var validator = new CancelItemCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await saleRepository.GetByIdAsync(command.SaleId, cancellationToken) ??
            throw new KeyNotFoundException($"Sale with ID {command.SaleId} not found");

        if (sale.IsCompleted())
            throw new InvalidOperationException($"Sale with ID {command.SaleId} is completed. Can't cancel items in a completed sale.");

        var productToCancel = sale.Products.FirstOrDefault(p => p.ProductId == command.ProductId && p.Status != Domain.Enums.SaleProductStatus.Deleted) ??
            throw new KeyNotFoundException($"Product with ID {command.ProductId} not found in sale {command.SaleId}");

        productToCancel.DefineDeleted();
        sale.CalculateTotalSaleAmount();
        sale.UpdatedAt = DateTimeOffset.UtcNow;

        await saleRepository.UpdateAsync(sale, cancellationToken);

        // Publish ItemCancelled event
        await eventPublisher.PublishAsync(new ItemCancelledEvent(productToCancel, sale), cancellationToken);

        return new CancelItemResult
        {
            Success = true,
            ProductId = command.ProductId,
            SaleId = command.SaleId
        };
    }
}
