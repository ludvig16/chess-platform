using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace ChessPlatform.Api.Tests.Auth;

public class AuthApiTests : IClassFixture<AuthWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthApiTests(AuthWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ShouldReturnUserNotFound_WhenUserDoesNotExist()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                Username = "not-in-the-database",
                Password = "1"
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var json = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(json);

        document.RootElement.GetProperty("code").GetString()
            .Should().Be("Auth.UserNotFound");

        document.RootElement.GetProperty("description").GetString()
            .Should().Be("User with this username doesn't exist");
    }
    
    [Fact]
    public async Task Login_ShouldReturnToken_WithValidCredentials()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                Username = "john",
                Password = "password123"
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}