using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

/// <summary>
/// Handler for processing DeleteSaleCommand requests
/// </summary>
/// <param name="saleRepository">The sale repository</param>
/// <param name="eventPublisher">The event publisher</param>
public class DeleteSaleHandler(ISaleRepository saleRepository, IEventPublisher eventPublisher)
    : IRequestHandler<DeleteSaleCommand, DeleteSaleResponse>
{
    /// <summary>
    /// Handles the DeleteSaleCommand request
    /// </summary>
    /// <param name="request">The DeleteSale command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the delete operation</returns>
    public async Task<DeleteSaleResponse> Handle(DeleteSaleCommand request, CancellationToken cancellationToken)
    {
        var validator = new DeleteSaleValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await saleRepository.GetByIdAsync(request.Id, cancellationToken) ??
            throw new KeyNotFoundException($"Sale with ID {request.Id} not found");

        if (sale.IsCompleted())
            throw new InvalidOperationException($"Sale with ID {request.Id} is completed. Can't delete a completed sale.");

        sale.DefineDeleted();

        await saleRepository.UpdateAsync(sale, cancellationToken);

        // Publish SaleCancelled event
        await eventPublisher.PublishAsync(new SaleCancelledEvent(sale), cancellationToken);

        return new DeleteSaleResponse { Success = true };
    }
}
