using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

/// <summary>
/// Validator for Cart
/// </summary>
/// <remarks>
/// Validation rules include:
/// - UserId: must a valid guid
/// - Date: must be greater than today
/// - Products: cannot be empty and all products must be valid (not null)
/// </remarks>
public class CartValidator : AbstractValidator<Cart>
{
    /// <inheritdoc />
    public CartValidator()
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