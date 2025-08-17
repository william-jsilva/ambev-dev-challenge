using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Specifications;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;

/// <summary>
/// Handler for processing user authentication requests.
/// </summary>
public class AuthenticateUserHandler : IRequestHandler<AuthenticateUserCommand, AuthenticateUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    /// <inheritdoc />
    public AuthenticateUserHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    /// <summary>
    /// Handles the authentication command to validate user credentials and generate a JWT token.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task<AuthenticateUserResult> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.Password))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var activeUserSpec = new ActiveUserSpecification();
        if (!activeUserSpec.IsSatisfiedBy(user))
        {
            throw new UnauthorizedAccessException("User is not active");
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthenticateUserResult
        {
            Token = token,
            Email = user.Email,
            Name = user.Username,
            Role = user.Role.ToString()
        };
    }
}
