using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

/// <summary>
/// Validator for UpdateUserRequest that defines validation rules for user creation.
/// </summary>
public class UpdateSaleRequestValidator : AbstractValidator<UpdateSaleRequest>
{
    /// <summary>
    /// Initializes a new instance of the UpdateSaleRequestValidator with defined validation rules.
    /// </summary>
    /// <remarks>
    /// Validation rules include:
    /// - UserId: must a valid guid
    /// - Date: cannot be empty
    /// - Products: cannot be empty and all products must be valid (not null)
    /// </remarks>
    public UpdateSaleRequestValidator()
    {
        RuleFor(user => user.UserId)
            .Must(userId => Guid.Empty != userId).WithMessage("UserId must a valid guid");

        RuleFor(user => user.Date)
            .NotEmpty().WithMessage("Date must not be empty.");

        RuleFor(user => user.Products)
            .NotEmpty().WithMessage("Products cannot be empty.")
            .Must(products => products.All(product => product != null)).WithMessage("All products must be valid.");
    }
}