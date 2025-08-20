using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Validator for CreateSaleCommand that defines validation rules for user creation command.
/// </summary>
public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    /// <summary>
    /// Initializes a new instance of the CreateSaleCommandValidator with defined validation rules.
    /// </summary>
    /// <remarks>
    /// Validation rules include:
    /// - UserId: must a valid guid
    /// - Date: must be greater than today
    /// - Products: cannot be empty and all products must be valid (not null)
    /// </remarks>
    public CreateSaleCommandValidator()
    {
        RuleFor(user => user.CartId)
            .Must(userId => Guid.Empty != userId).WithMessage("CartId must a valid guid");

        RuleFor(user => user.Date)
            .GreaterThanOrEqualTo(DateTimeOffset.Now.Date).WithMessage("Date must be greater or equal than today");

    }
}