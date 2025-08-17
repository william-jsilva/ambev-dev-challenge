using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Entities;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.CreateCart;

/// <summary>
/// Command for creating a new cart.
/// </summary>
/// <remarks>
/// This command is used to capture the required data for creating a cart, 
/// including userId, date and a list of products.
/// It implements <see cref="IRequest{TResponse}"/> to initiate the request 
/// that returns a <see cref="CreateCartResult"/>.
/// 
/// The data provided in this command is validated using the 
/// <see cref="CreateCartCommandValidator"/> which extends 
/// populated and follow the required rules.
/// </remarks>
public struct CreateCartCommand(Guid userId, DateTimeOffset date, IEnumerable<CartProduct> products) : IRequest<CreateCartResult>
{
    /// <summary>
    /// User Id
    /// </summary>
    public Guid UserId { get; private set; } = userId;

    /// <summary>
    /// Date of cart
    /// </summary>
    public DateTimeOffset Date { get; private set; } = date;

    /// <summary>
    /// List of produts
    /// </summary>
    public IEnumerable<CartProduct> Products { get; private set; } = products;

    /// <summary>
    /// Validates the command data.
    /// </summary>
    /// <returns></returns>
    public readonly ValidationResultDetail Validate()
    {
        var validator = new CreateCartCommandValidator();
        var result = validator.Validate(this);

        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }
}