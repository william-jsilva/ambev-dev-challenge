using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelItem;

/// <summary>
/// Validator for CancelItemRequest
/// </summary>
public class CancelItemRequestValidator : AbstractValidator<CancelItemRequest>
{
    /// <inheritdoc />
    public CancelItemRequestValidator()
    {
        RuleFor(request => request.SaleId)
            .NotEmpty().WithMessage("SaleId is required");

        RuleFor(request => request.ProductId)
            .NotEmpty().WithMessage("ProductId is required");
    }
}

