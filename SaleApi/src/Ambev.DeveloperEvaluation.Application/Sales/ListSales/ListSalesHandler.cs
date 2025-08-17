using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>
/// Handler for processing ListSalesCommand requests
/// </summary>
/// <param name="saleRepository">The sale repository</param>
public class ListSalesHandler(ISaleRepository saleRepository) : IRequestHandler<ListSalesCommand, ListSalesResult>
{
    /// <summary>
    /// Handles the ListSalesCommand request
    /// </summary>
    /// <param name="request">The ListSales command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>page with sales</returns>
    public async Task<ListSalesResult> Handle(ListSalesCommand request, CancellationToken cancellationToken)
    {
        var validator = new ListSalesValidator(); // ToDo: avaliar como n�o depender de uma inst�ncia espec�fica (SOLID) sem quebrar a estrutura da Ambev [Interface?]
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sales = await saleRepository.GetAsync(request.Page, request.Size, request.Order, cancellationToken) ?? [];
        var totalItems = await saleRepository.GetTotalAsync(cancellationToken);

        return new ListSalesResult
        {
            Sales = sales,
            TotalItems = totalItems
        };
    }
}
