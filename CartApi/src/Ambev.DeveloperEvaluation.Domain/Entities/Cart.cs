using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Validation;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a cart in the system.
/// This entity follows domain-driven design principles and includes business rules validation.
/// </summary>
public class Cart : BaseEntity
{
    /// <inheritdoc />
    public Cart()
    {
        CreatedAt = DateTimeOffset.UtcNow;
        Status = CartStatus.Active;
    }

    /// <summary>
    /// User Id
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Date of cart
    /// </summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>
    /// The current status of the cart
    /// </summary>
    public CartStatus Status { get; set; }

    /// <summary>
    /// Gets the date and time when the user was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets the date and time of the last update to the cart's information.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Gets the date and time of deleted cart
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// List of <see cref="CartProduct "/>
    /// </summary>
    public List<CartProduct> Products { get; set; } = [];

    /// <summary>
    /// Changes the status of the cart deleted.
    /// </summary>
    public void DefineDeleted()
    {
        Status = CartStatus.Deleted;
        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Verify if the cart is completed.
    /// </summary>
    /// <returns></returns>
    public bool IsCompleted()
    {
        return Status == CartStatus.Completed;
    }

    /// <summary>
    /// Performs validation of the cart entity using the <see cref="CartValidator "/> rules.
    /// </summary>
    /// <returns>
    /// A <see cref="ValidationResultDetail"/> containing:
    /// - IsValid: Indicates whether all validation rules passed
    /// - Errors: Collection of validation errors if any rules failed
    /// </returns>
    public ValidationResultDetail Validate()
    {
        var validator = new CartValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(error => (ValidationErrorDetail)error)
        };
    }
}