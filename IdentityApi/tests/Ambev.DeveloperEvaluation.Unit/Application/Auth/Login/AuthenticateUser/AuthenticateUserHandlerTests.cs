using Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Bogus;
using FluentAssertions;
using MediatR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Auth.AuthenticateUser;

public class AuthenticateUserHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly AuthenticateUserHandler _handler;
    private readonly Faker _faker;

    public AuthenticateUserHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _handler = new AuthenticateUserHandler(_userRepository, _passwordHasher, _jwtTokenGenerator);
        _faker = new Faker("pt_BR");
    }

    [Fact]
    public async Task Handle_ValidCredentials_ShouldReturnSuccessResult()
    {
        // Arrange
        var email = "joao.silva@email.com";
        var password = "Senha@123";
        var hashedPassword = "hashed_password_123";
        var token = "jwt_token_123";

        var command = new AuthenticateUserCommand { Email = email, Password = password };
        var user = CreateValidUser(email, hashedPassword);

        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(password, hashedPassword).Returns(true);
        _jwtTokenGenerator.GenerateToken(user).Returns(token);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(token);
        result.Email.Should().Be(user.Email);
        result.Name.Should().Be(user.Username);
        result.Role.Should().Be(user.Role.ToString());

        await _userRepository.Received(1).GetByEmailAsync(email, Arg.Any<CancellationToken>());
        _passwordHasher.Received(1).VerifyPassword(password, hashedPassword);
        _jwtTokenGenerator.Received(1).GenerateToken(user);
    }

    [Fact]
    public async Task Handle_InvalidEmail_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var email = "invalid@email.com";
        var password = "Senha@123";

        var command = new AuthenticateUserCommand { Email = email, Password = password };

        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>()).Returns((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("Invalid credentials");

        await _userRepository.Received(1).GetByEmailAsync(email, Arg.Any<CancellationToken>());
        _passwordHasher.DidNotReceive().VerifyPassword(Arg.Any<string>(), Arg.Any<string>());
        _jwtTokenGenerator.DidNotReceive().GenerateToken(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_InvalidPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var email = "joao.silva@email.com";
        var password = "WrongPassword";
        var hashedPassword = "hashed_password_123";

        var command = new AuthenticateUserCommand { Email = email, Password = password };
        var user = CreateValidUser(email, hashedPassword);

        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(password, hashedPassword).Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("Invalid credentials");

        await _userRepository.Received(1).GetByEmailAsync(email, Arg.Any<CancellationToken>());
        _passwordHasher.Received(1).VerifyPassword(password, hashedPassword);
        _jwtTokenGenerator.DidNotReceive().GenerateToken(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_InactiveUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var email = "joao.silva@email.com";
        var password = "Senha@123";
        var hashedPassword = "hashed_password_123";

        var command = new AuthenticateUserCommand { Email = email, Password = password };
        var user = CreateInactiveUser(email, hashedPassword);

        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(password, hashedPassword).Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("User is not active");

        await _userRepository.Received(1).GetByEmailAsync(email, Arg.Any<CancellationToken>());
        _passwordHasher.Received(1).VerifyPassword(password, hashedPassword);
        _jwtTokenGenerator.DidNotReceive().GenerateToken(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_SuspendedUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var email = "joao.silva@email.com";
        var password = "Senha@123";
        var hashedPassword = "hashed_password_123";

        var command = new AuthenticateUserCommand { Email = email, Password = password };
        var user = CreateSuspendedUser(email, hashedPassword);

        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(password, hashedPassword).Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("User is not active");

        await _userRepository.Received(1).GetByEmailAsync(email, Arg.Any<CancellationToken>());
        _passwordHasher.Received(1).VerifyPassword(password, hashedPassword);
        _jwtTokenGenerator.DidNotReceive().GenerateToken(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var email = "joao.silva@email.com";
        var password = "Senha@123";

        var command = new AuthenticateUserCommand { Email = email, Password = password };
        var exception = new InvalidOperationException("Database connection failed");

        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>()).ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        thrownException.Should().Be(exception);

        await _userRepository.Received(1).GetByEmailAsync(email, Arg.Any<CancellationToken>());
        _passwordHasher.DidNotReceive().VerifyPassword(Arg.Any<string>(), Arg.Any<string>());
        _jwtTokenGenerator.DidNotReceive().GenerateToken(Arg.Any<User>());
    }

    private static User CreateValidUser(string email, string hashedPassword)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = "joao.silva",
            Email = email,
            Phone = "(11) 99999-9999",
            Password = hashedPassword,
            Role = UserRole.Customer,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static User CreateInactiveUser(string email, string hashedPassword)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = "joao.silva",
            Email = email,
            Phone = "(11) 99999-9999",
            Password = hashedPassword,
            Role = UserRole.Customer,
            Status = UserStatus.Inactive,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static User CreateSuspendedUser(string email, string hashedPassword)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = "joao.silva",
            Email = email,
            Phone = "(11) 99999-9999",
            Password = hashedPassword,
            Role = UserRole.Customer,
            Status = UserStatus.Suspended,
            CreatedAt = DateTime.UtcNow
        };
    }
}
