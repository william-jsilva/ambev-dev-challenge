using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Ambev.DeveloperEvaluation.Integration;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Repositories;

public class UserRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public UserRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateAsync_ValidUser_ShouldCreateUserSuccessfully()
    {
        // Arrange
        using var context = _fixture.GetContext();
        var repository = new UserRepository(context);
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            Phone = "(11) 99999-9999",
            Password = "Test@123",
            Role = UserRole.Customer,
            Status = UserStatus.Active
        };

        // Act
        var result = await repository.CreateAsync(user, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Username.Should().Be("testuser");
        result.Email.Should().Be("test@example.com");
        result.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ShouldReturnUser()
    {
        // Arrange
        using var context = _fixture.GetContext();
        var repository = new UserRepository(context);
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            Phone = "(11) 99999-9999",
            Password = "Test@123",
            Role = UserRole.Customer,
            Status = UserStatus.Active
        };

        var createdUser = await repository.CreateAsync(user, CancellationToken.None);

        // Act
        var result = await repository.GetByIdAsync(createdUser.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdUser.Id);
        result.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingUser_ShouldReturnNull()
    {
        // Arrange
        using var context = _fixture.GetContext();
        var repository = new UserRepository(context);
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistingId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_ExistingUser_ShouldReturnUser()
    {
        // Arrange
        using var context = _fixture.GetContext();
        var repository = new UserRepository(context);
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            Phone = "(11) 99999-9999",
            Password = "Test@123",
            Role = UserRole.Customer,
            Status = UserStatus.Active
        };

        await repository.CreateAsync(user, CancellationToken.None);

        // Act
        var result = await repository.GetByEmailAsync("test@example.com", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
        result.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetByEmailAsync_NonExistingUser_ShouldReturnNull()
    {
        // Arrange
        using var context = _fixture.GetContext();
        var repository = new UserRepository(context);

        // Act
        var result = await repository.GetByEmailAsync("nonexisting@example.com", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingUser_ShouldDeleteUserSuccessfully()
    {
        // Arrange
        using var context = _fixture.GetContext();
        var repository = new UserRepository(context);
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            Phone = "(11) 99999-9999",
            Password = "Test@123",
            Role = UserRole.Customer,
            Status = UserStatus.Active
        };

        var createdUser = await repository.CreateAsync(user, CancellationToken.None);

        // Act
        var result = await repository.DeleteAsync(createdUser.Id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var deletedUser = await repository.GetByIdAsync(createdUser.Id, CancellationToken.None);
        deletedUser.Should().BeNull();
    }




}
