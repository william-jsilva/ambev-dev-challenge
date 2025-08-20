using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

/// <summary>
/// Handler for processing GetSaleCommand requests
/// </summary>
/// <param name="saleRepository">The sale repository</param>
/// <param name="mapper">The AutoMapper instance</param>
public class GetSaleHandler(ISaleRepository saleRepository, IMapper mapper)
    : IRequestHandler<GetSaleCommand, GetSaleResult>
{
    /// <summary>
    /// Handles the GetSaleCommand request
    /// </summary>
    /// <param name="request">The GetSale command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sale details if found</returns>
    public async Task<GetSaleResult> Handle(GetSaleCommand request, CancellationToken cancellationToken)
    {
        var validator = new GetSaleValidator(); // ToDo: avaliar como n�o depender de uma inst�ncia espec�fica (SOLID) sem quebrar a estrutura da Ambev [Interface?]
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await saleRepository.GetByIdAsync(request.Id, cancellationToken) ??
            throw new KeyNotFoundException($"Sale with ID {request.Id} not found");

        return mapper.Map<GetSaleResult>(sale);
    }
}
