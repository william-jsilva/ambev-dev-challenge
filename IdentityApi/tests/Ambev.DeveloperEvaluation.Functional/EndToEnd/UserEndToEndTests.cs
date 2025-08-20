using Ambev.DeveloperEvaluation.Functional;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.EndToEnd;

public class UserEndToEndTests : IClassFixture<WebApplicationFactory>
{
    private readonly WebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UserEndToEndTests(WebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateUser_ValidData_ShouldReturnSuccess()
    {
        // Arrange
        var userData = new
        {
            Username = "testuser",
            Email = "test@example.com",
            Phone = "(11) 99999-9999",
            Password = "Test@123",
            Status = "Active",
            Role = "Customer"
        };

        var json = JsonSerializer.Serialize(userData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/users", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateUser_InvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var userData = new
        {
            Username = "", // Invalid username
            Email = "invalid-email", // Invalid email
            Phone = "123", // Invalid phone
            Password = "123", // Invalid password
            Status = "Active",
            Role = "Customer"
        };

        var json = JsonSerializer.Serialize(userData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/users", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUser_DuplicateEmail_ShouldReturnConflict()
    {
        // Arrange
        var userData = new
        {
            Username = "testuser1",
            Email = "duplicate@example.com",
            Phone = "(11) 99999-9999",
            Password = "Test@123",
            Status = "Active",
            Role = "Customer"
        };

        var json = JsonSerializer.Serialize(userData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Create first user
        await _client.PostAsync("/api/users", content);

        // Create second user with same email
        var userData2 = new
        {
            Username = "testuser2",
            Email = "duplicate@example.com", // Same email
            Phone = "(11) 88888-8888",
            Password = "Test@123",
            Status = "Active",
            Role = "Customer"
        };

        var json2 = JsonSerializer.Serialize(userData2);
        var content2 = new StringContent(json2, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/users", content2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AuthenticateUser_ValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var userData = new
        {
            Username = "authuser",
            Email = "auth@example.com",
            Phone = "(11) 99999-9999",
            Password = "Test@123",
            Status = "Active",
            Role = "Customer"
        };

        var json = JsonSerializer.Serialize(userData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Create user first
        await _client.PostAsync("/api/users", content);

        var loginData = new
        {
            Email = "auth@example.com",
            Password = "Test@123"
        };

        var loginJson = JsonSerializer.Serialize(loginData);
        var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", loginContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("token");
    }

    [Fact]
    public async Task AuthenticateUser_InvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginData = new
        {
            Email = "nonexisting@example.com",
            Password = "WrongPassword"
        };

        var loginJson = JsonSerializer.Serialize(loginData);
        var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", loginContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUser_ExistingUser_ShouldReturnUser()
    {
        // Arrange
        var userData = new
        {
            Username = "getuser",
            Email = "get@example.com",
            Phone = "(11) 99999-9999",
            Password = "Test@123",
            Status = "Active",
            Role = "Customer"
        };

        var json = JsonSerializer.Serialize(userData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Create user first
        var createResponse = await _client.PostAsync("/api/users", content);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdUser = JsonSerializer.Deserialize<JsonElement>(createResponseContent);
        var userId = createdUser.GetProperty("id").GetString();

        // Act
        var response = await _client.GetAsync($"/api/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("getuser");
        responseContent.Should().Contain("get@example.com");
    }

    [Fact]
    public async Task GetUser_NonExistingUser_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid().ToString();

        // Act
        var response = await _client.GetAsync($"/api/users/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUser_ExistingUser_ShouldUpdateSuccessfully()
    {
        // Arrange
        var userData = new
        {
            Username = "updateuser",
            Email = "update@example.com",
            Phone = "(11) 99999-9999",
            Password = "Test@123",
            Status = "Active",
            Role = "Customer"
        };

        var json = JsonSerializer.Serialize(userData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Create user first
        var createResponse = await _client.PostAsync("/api/users", content);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdUser = JsonSerializer.Deserialize<JsonElement>(createResponseContent);
        var userId = createdUser.GetProperty("id").GetString();

        var updateData = new
        {
            Username = "updateduser",
            Email = "updated@example.com",
            Phone = "(11) 88888-8888",
            Status = "Suspended",
            Role = "Admin"
        };

        var updateJson = JsonSerializer.Serialize(updateData);
        var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/users/{userId}", updateContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("updateduser");
        responseContent.Should().Contain("updated@example.com");
    }

    [Fact]
    public async Task DeleteUser_ExistingUser_ShouldDeleteSuccessfully()
    {
        // Arrange
        var userData = new
        {
            Username = "deleteuser",
            Email = "delete@example.com",
            Phone = "(11) 99999-9999",
            Password = "Test@123",
            Status = "Active",
            Role = "Customer"
        };

        var json = JsonSerializer.Serialize(userData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Create user first
        var createResponse = await _client.PostAsync("/api/users", content);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdUser = JsonSerializer.Deserialize<JsonElement>(createResponseContent);
        var userId = createdUser.GetProperty("id").GetString();

        // Act
        var response = await _client.DeleteAsync($"/api/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify user is deleted
        var getResponse = await _client.GetAsync($"/api/users/{userId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnUsersList()
    {
        // Arrange
        var userData1 = new
        {
            Username = "listuser1",
            Email = "list1@example.com",
            Phone = "(11) 99999-9999",
            Password = "Test@123",
            Status = "Active",
            Role = "Customer"
        };

        var userData2 = new
        {
            Username = "listuser2",
            Email = "list2@example.com",
            Phone = "(11) 88888-8888",
            Password = "Test@123",
            Status = "Active",
            Role = "Admin"
        };

        var json1 = JsonSerializer.Serialize(userData1);
        var content1 = new StringContent(json1, Encoding.UTF8, "application/json");
        var json2 = JsonSerializer.Serialize(userData2);
        var content2 = new StringContent(json2, Encoding.UTF8, "application/json");

        // Create users
        await _client.PostAsync("/api/users", content1);
        await _client.PostAsync("/api/users", content2);

        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("listuser1");
        responseContent.Should().Contain("listuser2");
    }
}
