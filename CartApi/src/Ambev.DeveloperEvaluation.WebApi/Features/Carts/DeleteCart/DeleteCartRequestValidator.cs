using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts.DeleteCart;

/// <summary>
/// Validator for DeleteCartRequest
/// </summary>
public class DeleteCartRequestValidator : AbstractValidator<DeleteCartRequest>
{
    /// <summary>
    /// Initializes validation rules for DeleteCartRequest
    /// </summary>
    public DeleteCartRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Cart ID is required");
    }
}
