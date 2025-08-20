using Ambev.DeveloperEvaluation.Common.Validation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelItem;

/// <summary>
/// Command for cancelling a specific item in a sale
/// </summary>
public struct CancelItemCommand(Guid saleId, Guid productId) : IRequest<CancelItemResult>
{
    /// <summary>
    /// The ID of the sale containing the item to cancel
    /// </summary>
    public Guid SaleId { get; private set; } = saleId;

    /// <summary>
    /// The ID of the product to cancel
    /// </summary>
    public Guid ProductId { get; private set; } = productId;

    /// <summary>
    /// Validates the command data
    /// </summary>
    /// <returns>Validation result</returns>
    public readonly ValidationResultDetail Validate()
    {
        var validator = new CancelItemCommandValidator();
        var result = validator.Validate(this);

        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }
}

