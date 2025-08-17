using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Entities;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Command for creating a new sale.
/// </summary>
/// <remarks>
/// This command is used to capture the required data for creating a sale, 
/// including userId, date and a list of products.
/// It implements <see cref="IRequest{TResponse}"/> to initiate the request 
/// that returns a <see cref="CreateSaleResult"/>.
/// 
/// The data provided in this command is validated using the 
/// <see cref="CreateSaleCommandValidator"/> which extends 
/// populated and follow the required rules.
/// </remarks>
public struct CreateSaleCommand(Guid userId, DateTimeOffset date, IEnumerable<SaleProduct> products) : IRequest<CreateSaleResult>
{
    /// <summary>
    /// User Id
    /// </summary>
    public Guid UserId { get; private set; } = userId;

    /// <summary>
    /// Date of sale
    /// </summary>
    public DateTimeOffset Date { get; private set; } = date;

    /// <summary>
    /// List of produts
    /// </summary>
    public IEnumerable<SaleProduct> Products { get; private set; } = products;

    /// <summary>
    /// Validates the command data.
    /// </summary>
    /// <returns></returns>
    public readonly ValidationResultDetail Validate()
    {
        var validator = new CreateSaleCommandValidator();
        var result = validator.Validate(this);

        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }
}