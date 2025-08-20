using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelItem;

/// <summary>
/// Validator for CancelItemCommand
/// </summary>
public class CancelItemCommandValidator : AbstractValidator<CancelItemCommand>
{
    /// <inheritdoc />
    public CancelItemCommandValidator()
    {
        RuleFor(command => command.SaleId)
            .NotEmpty().WithMessage("SaleId is required");

        RuleFor(command => command.ProductId)
            .NotEmpty().WithMessage("ProductId is required");
    }
}
