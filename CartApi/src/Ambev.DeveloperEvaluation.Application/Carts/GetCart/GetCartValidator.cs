using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Carts.GetCart;

/// <summary>
/// Validator for GetCartCommand
/// </summary>
public class GetCartValidator : AbstractValidator<GetCartCommand>
{
    /// <summary>
    /// Initializes validation rules for GetCartCommand
    /// </summary>
    public GetCartValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Cart ID is required");
    }
}
