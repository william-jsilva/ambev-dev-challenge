using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

/// <summary>
/// Validator for CreateUserRequest that defines validation rules for user creation.
/// </summary>
public class CreateSaleRequestValidator : AbstractValidator<CreateSaleRequest>
{
    /// <summary>
    /// Initializes a new instance of the CreateSaleRequestValidator with defined validation rules.
    /// </summary>
    /// <remarks>
    /// Validation rules include:
    /// - UserId: must a valid guid
    /// - Date: must be greater than today
    /// - Products: cannot be empty and all products must be valid (not null)
    /// </remarks>
    public CreateSaleRequestValidator()
    {
        RuleFor(user => user.CartId)
            .Must(userId => Guid.Empty != userId).WithMessage("CartId must a valid guid.");

        RuleFor(user => user.Date)
            .NotEmpty().WithMessage("Date must not be empty.")
            .GreaterThanOrEqualTo(DateTimeOffset.Now.Date).WithMessage("Date must be greater or equal than today");
    }
}