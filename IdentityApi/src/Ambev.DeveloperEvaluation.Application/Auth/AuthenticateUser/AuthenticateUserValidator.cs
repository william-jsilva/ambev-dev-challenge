using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;

/// <summary>
/// Validator for AuthenticateUserCommand
/// </summary>
public class AuthenticateUserValidator : AbstractValidator<AuthenticateUserCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticateUserValidator"/> class.
    /// </summary>
    public AuthenticateUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6);
    }
}
