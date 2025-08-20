using Ambev.DeveloperEvaluation.Common.Validation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Command for creating a new sale.
/// </summary>
/// <remarks>
/// This command is used to capture the required data for creating a sale, 
/// including userId, date and a cartId.
/// The data provided in this command is validated using the 
/// <see cref="CreateSaleCommandValidator"/> which extends 
/// populated and follow the required rules.
/// </remarks>
public struct CreateSaleCommand(Guid cartId, DateTimeOffset date, string branch) : IRequest<CreateSaleResult>
{
    /// <summary>
    /// Cart Id associated with the sale
    /// </summary>
    public Guid CartId { get; private set; } = cartId;

    /// <summary>
    /// Date of sale
    /// </summary>
    public DateTimeOffset Date { get; private set; } = date;

    /// <summary>
    /// Branch where the sale was made
    /// </summary>
    public string Branch { get; set; } = branch;

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