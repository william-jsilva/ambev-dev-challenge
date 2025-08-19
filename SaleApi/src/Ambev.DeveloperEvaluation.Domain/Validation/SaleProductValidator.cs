using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

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
public class SaleProductValidator : AbstractValidator<SaleProduct>
{
    /// <inheritdoc />
    public SaleProductValidator()
    {
        const decimal maxPercentDiscount = 0.8M; 
        const decimal zeroPercentDiscount = 1M; 

        RuleFor(sale => sale.ProductId)
            .Must(productId => Guid.Empty != productId).WithMessage("ProductId must a valid guid");

        RuleFor(sale => sale.Quantity)
            .GreaterThanOrEqualTo(1).WithMessage("Date must be greater or equal than 1");

        RuleFor(sale => sale.UnitPrice)
            .GreaterThan(0).WithMessage("Date must be greater than 0");

        RuleFor(sale => sale.Discounts)
            .GreaterThan(maxPercentDiscount).WithMessage("Discounts must be less than 80% of the total price")
            .LessThanOrEqualTo(zeroPercentDiscount).WithMessage("Discounts must be greater than or equal to 0% of the total price");

        RuleFor(sale => sale.TotalAmount)
            .GreaterThan(0).WithMessage("TotalAmount must be greater than 0");

        RuleFor(sale => sale.Status)
            .IsInEnum().WithMessage("Status must be a valid SaleProductStatus")
            .Equal(SaleProductStatus.Active).WithMessage("Status must be Active");

        RuleFor(sale => sale.CreatedAt)
            .GreaterThan(DateTimeOffset.MinValue).WithMessage("CreatedAt must be greater than DateTimeOffset.MinValue");
    }
}