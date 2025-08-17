using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Handler for processing CreateSaleCommand requests
/// </summary>
/// <param name="saleRepository">The sale repository</param>
/// <param name="mapper">The AutoMapper instance</param>
public class CreateSaleHandler(ISaleRepository saleRepository, IMapper mapper) : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    /// <summary>
    /// Handles the CreateSaleCommand request
    /// </summary>
    /// <param name="command">The CreateSale command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created sale details</returns>
    public async Task<CreateSaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var existingSale = await saleRepository.GetActiveByUserIdAsync(command.UserId, cancellationToken);
        if (existingSale != null)
            throw new InvalidOperationException($"Sale with userId {command.UserId} already exists");

        var sale = mapper.Map<Sale>(command);

        var createdSale = await saleRepository.CreateAsync(sale, cancellationToken);
        var result = mapper.Map<CreateSaleResult>(createdSale);

        return result;
    }
}
