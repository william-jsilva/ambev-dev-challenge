using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

/// <summary>
/// Validator for Sale
/// </summary>
/// <remarks>
/// Validation rules include:
/// - UserId: must a valid guid
/// - Date: must be greater than today
/// - Products: cannot be empty and all products must be valid (not null)
/// </remarks>
public class SaleValidator : AbstractValidator<Sale>
{
    /// <inheritdoc />
    public SaleValidator()
    {
        const int maxProductsCount = 20;

        RuleFor(sale => sale.UserId)
            .Must(userId => Guid.Empty != userId).WithMessage("UserId must a valid guid");

        RuleFor(sale => sale.Date)
            .GreaterThanOrEqualTo(DateTimeOffset.Now.Date).WithMessage("Date must be greater or equal than today");

        RuleFor(sale => sale.Products)
            .NotEmpty().WithMessage("Products cannot be empty.")
            .Must(products => products.All(product => product != null)).WithMessage("All products must be valid.");

        RuleFor(sale => sale.Products)
            .NotNull().WithMessage("Products can't be null")
            .Must(list => list.Count <= maxProductsCount).WithMessage($"Products count must be less than or equal to {maxProductsCount}");

        RuleForEach(sale => sale.Products)
            .NotNull().WithMessage("Product can't be null")
            .SetValidator(new SaleProductValidator());
    }
}