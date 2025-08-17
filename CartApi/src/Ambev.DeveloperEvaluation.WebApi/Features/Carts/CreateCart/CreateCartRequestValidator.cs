using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts.CreateCart;

/// <summary>
/// Validator for CreateUserRequest that defines validation rules for user creation.
/// </summary>
public class CreateCartRequestValidator : AbstractValidator<CreateCartRequest>
{
    /// <summary>
    /// Initializes a new instance of the CreateCartRequestValidator with defined validation rules.
    /// </summary>
    /// <remarks>
    /// Validation rules include:
    /// - UserId: must a valid guid
    /// - Date: must be greater than today
    /// - Products: cannot be empty and all products must be valid (not null)
    /// </remarks>
    public CreateCartRequestValidator()
    {
        RuleFor(user => user.UserId)
            .Must(userId => Guid.Empty != userId).WithMessage("UserId must a valid guid.");

        RuleForEach(user => user.Products).ChildRules(product =>
        {
            product.RuleFor(p => p.ProductId)
                .Must(productId => Guid.Empty != productId)
                .WithMessage(p => "ProductId must a valid guid.");
        });

        RuleForEach(user => user.Products).ChildRules(product =>
        {
            product.RuleFor(p => p.Quantity)
                .GreaterThan(0)
                .WithMessage(p => $"Product '{p.ProductId}' has an invalid quantity: {p.Quantity}.");
        });

        RuleFor(user => user.Date)
            .NotEmpty().WithMessage("Date must not be empty.")
            .GreaterThanOrEqualTo(DateTimeOffset.Now.Date).WithMessage("Date must be greater or equal than today");

        RuleFor(user => user.Products)
            .NotEmpty().WithMessage("Products cannot be empty.")
            .Must(products => products.All(product => product != null)).WithMessage("All products must be valid.");
    }
}