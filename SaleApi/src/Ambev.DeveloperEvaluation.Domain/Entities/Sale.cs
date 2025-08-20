using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Validation;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a sale in the system.
/// This entity follows domain-driven design principles and includes business rules validation.
/// </summary>
public class Sale : BaseEntity
{
    /// <inheritdoc />
    public Sale()
    {
        CreatedAt = DateTimeOffset.UtcNow;
        Status = SaleStatus.Active;
    }

    /// <summary>
    /// Sale number
    /// </summary>
    public long SaleNumber { get; set; }

    /// <summary>
    /// Total amount of the sale
    /// </summary>
    public decimal TotalSaleAmount { get; set; }

    /// <summary>
    /// Branch where the sale was made
    /// </summary>
    public string Branch { get; set; }

    /// <summary>
    /// User Id
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Date of sale
    /// </summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>
    /// The current status of the sale
    /// </summary>
    public SaleStatus Status { get; set; }

    /// <summary>
    /// Gets the date and time when the user was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets the date and time of the last update to the sale's information.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Gets the date and time of deleted sale
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// List of <see cref="SaleProduct "/>
    /// </summary>
    public List<SaleProduct> Products { get; set; } = [];

    /// <summary>
    /// Calculates the discount for each product in the sale.
    /// </summary>
    public void CalculateTotalSaleAmount()
    {
        foreach (var product in Products)
        {
            product.CalculateDiscount();
            product.CalculateTotalAmount();
        }

        TotalSaleAmount = Products.Sum(product => product.TotalAmount);
    }

    /// <summary>
    /// Changes the status of the sale deleted.
    /// </summary>
    public void DefineDeleted()
    {
        Status = SaleStatus.Deleted;
        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Verify if the sale is completed.
    /// </summary>
    /// <returns></returns>
    public bool IsCompleted()
    {
        return Status == SaleStatus.Completed;
    }

    /// <summary>
    /// Performs validation of the sale entity using the <see cref="SaleValidator "/> rules.
    /// </summary>
    /// <returns>
    /// A <see cref="ValidationResultDetail"/> containing:
    /// - IsValid: Indicates whether all validation rules passed
    /// - Errors: Collection of validation errors if any rules failed
    /// </returns>
    public ValidationResultDetail Validate()
    {
        var validator = new SaleValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(error => (ValidationErrorDetail)error)
        };
    }
}