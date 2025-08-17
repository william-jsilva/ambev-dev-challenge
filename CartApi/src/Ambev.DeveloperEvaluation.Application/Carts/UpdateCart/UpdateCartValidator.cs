using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;

/// <summary>
/// Validator for UpdateCartCommand that defines validation rules for user creation command.
/// </summary>
public class UpdateCartCommandValidator : AbstractValidator<UpdateCartCommand>
{
    /// <summary>
    /// Initializes a new instance of the UpdateCartCommandValidator with defined validation rules.
    /// </summary>
    /// <remarks>
    /// Validation rules include:
    /// - UserId: must a valid guid
    /// - Date: must be greater than today
    /// - Products: cannot be empty and all products must be valid (not null)
    /// </remarks>
    public UpdateCartCommandValidator()
    {
        RuleFor(user => user.UserId)
            .Must(userId => Guid.Empty != userId).WithMessage("UserId must a valid guid");

        RuleFor(user => user.Date)
            .GreaterThanOrEqualTo(DateTimeOffset.Now.Date).WithMessage("Date must be greater or equal than today");

        RuleFor(user => user.Products)
            .NotEmpty().WithMessage("Products cannot be empty.")
            .Must(products => products.All(product => product != null)).WithMessage("All products must be valid.");
    }
}