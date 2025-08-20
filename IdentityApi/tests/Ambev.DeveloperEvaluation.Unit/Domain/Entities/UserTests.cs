using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Bogus;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class UserTests
{
    private readonly Faker _faker;

    public UserTests()
    {
        _faker = new Faker("pt_BR");
    }

    [Fact]
    public void Validate_WithValidUser_ShouldReturnValidResult()
    {
        // Arrange
        var user = new User
        {
            Username = "joao.silva",
            Email = "joao.silva@email.com",
            Phone = "(11) 99999-9999",
            Password = "Senha@123",
            Role = UserRole.Customer,
            Status = UserStatus.Active
        };

        // Act
        var result = user.Validate();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("jo")]
    public void Validate_WithInvalidUsername_ShouldReturnValidationError(string? username)
    {
        // Arrange
        var user = new User
        {
            Username = username,
            Email = "joao.silva@email.com",
            Phone = "(11) 99999-9999",
            Password = "Senha@123",
            Role = UserRole.Customer
        };

        // Act
        var result = user.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Error.Should().Contain("Username");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("invalid-email")]
    [InlineData("joao@")]
    [InlineData("@email.com")]
    public void Validate_WithInvalidEmail_ShouldReturnValidationError(string? email)
    {
        // Arrange
        var user = new User
        {
            Username = "joao.silva",
            Email = email,
            Phone = "(11) 99999-9999",
            Password = "Senha@123",
            Role = UserRole.Customer
        };

        // Act
        var result = user.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Error.Should().Contain("Email");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("123456789")]
    [InlineData("(11) 99999-999")]
    [InlineData("11 99999-9999")]
    public void Validate_WithInvalidPhone_ShouldReturnValidationError(string? phone)
    {
        // Arrange
        var user = new User
        {
            Username = "joao.silva",
            Email = "joao.silva@email.com",
            Phone = phone,
            Password = "Senha@123",
            Role = UserRole.Customer
        };

        // Act
        var result = user.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Error.Should().Contain("Phone");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("123")]
    [InlineData("senha")]
    [InlineData("SENHA")]
    [InlineData("Senha123")]
    public void Validate_WithInvalidPassword_ShouldReturnValidationError(string? password)
    {
        // Arrange
        var user = new User
        {
            Username = "joao.silva",
            Email = "joao.silva@email.com",
            Phone = "(11) 99999-9999",
            Password = password,
            Role = UserRole.Customer
        };

        // Act
        var result = user.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Error.Should().Contain("Password");
    }

    [Fact]
    public void Validate_WithInvalidRole_ShouldReturnValidationError()
    {
        // Arrange
        var user = new User
        {
            Username = "joao.silva",
            Email = "joao.silva@email.com",
            Phone = "(11) 99999-9999",
            Password = "Senha@123",
            Role = (UserRole)999 // Invalid role
        };

        // Act
        var result = user.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Error.Should().Contain("Role");
    }

    [Fact]
    public void Activate_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        var user = new User
        {
            Status = UserStatus.Inactive
        };

        var beforeActivation = DateTime.UtcNow;

        // Act
        user.Activate();

        // Assert
        user.Status.Should().Be(UserStatus.Active);
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeAfter(beforeActivation);
    }

    [Fact]
    public void Deactivate_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        var user = new User
        {
            Status = UserStatus.Active
        };

        var beforeDeactivation = DateTime.UtcNow;

        // Act
        user.Deactivate();

        // Assert
        user.Status.Should().Be(UserStatus.Inactive);
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeAfter(beforeDeactivation);
    }

    [Fact]
    public void Suspend_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        var user = new User
        {
            Status = UserStatus.Active
        };

        var beforeSuspension = DateTime.UtcNow;

        // Act
        user.Suspend();

        // Assert
        user.Status.Should().Be(UserStatus.Suspended);
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeAfter(beforeSuspension);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var user = new User();

        // Assert
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void IUser_Id_ShouldReturnStringRepresentation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId
        };

        // Act
        var result = ((IUser)user).Id;

        // Assert
        result.Should().Be(userId.ToString());
    }

    [Fact]
    public void IUser_Username_ShouldReturnUsername()
    {
        // Arrange
        var username = "joao.silva";
        var user = new User
        {
            Username = username
        };

        // Act
        var result = ((IUser)user).Username;

        // Assert
        result.Should().Be(username);
    }

    [Fact]
    public void IUser_Role_ShouldReturnStringRepresentation()
    {
        // Arrange
        var role = UserRole.Admin;
        var user = new User
        {
            Role = role
        };

        // Act
        var result = ((IUser)user).Role;

        // Assert
        result.Should().Be(role.ToString());
    }

    [Fact]
    public void Validate_WithMultipleValidationErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var user = new User
        {
            Username = "",
            Email = "invalid-email",
            Phone = "123",
            Password = "123",
            Role = (UserRole)999
        };

        // Act
        var result = user.Validate();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(5);
        result.Errors.Should().Contain(e => e.Error.Contains("Username"));
        result.Errors.Should().Contain(e => e.Error.Contains("Email"));
        result.Errors.Should().Contain(e => e.Error.Contains("Phone"));
        result.Errors.Should().Contain(e => e.Error.Contains("Password"));
        result.Errors.Should().Contain(e => e.Error.Contains("Role"));
    }

    [Theory]
    [InlineData("joao.silva", true)]
    [InlineData("maria.santos", true)]
    [InlineData("admin", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("jo", false)]
    public void Validate_WithVariousUsernames_ShouldValidateCorrectly(string? username, bool expectedIsValid)
    {
        // Arrange
        var user = new User
        {
            Username = username,
            Email = "joao.silva@email.com",
            Phone = "(11) 99999-9999",
            Password = "Senha@123",
            Role = UserRole.Customer
        };

        // Act
        var result = user.Validate();

        // Assert
        result.IsValid.Should().Be(expectedIsValid);
    }

    [Theory]
    [InlineData("joao.silva@email.com", true)]
    [InlineData("maria.santos@company.com", true)]
    [InlineData("admin@test.org", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("invalid-email", false)]
    [InlineData("joao@", false)]
    public void Validate_WithVariousEmails_ShouldValidateCorrectly(string? email, bool expectedIsValid)
    {
        // Arrange
        var user = new User
        {
            Username = "joao.silva",
            Email = email,
            Phone = "(11) 99999-9999",
            Password = "Senha@123",
            Role = UserRole.Customer
        };

        // Act
        var result = user.Validate();

        // Assert
        result.IsValid.Should().Be(expectedIsValid);
    }
}
