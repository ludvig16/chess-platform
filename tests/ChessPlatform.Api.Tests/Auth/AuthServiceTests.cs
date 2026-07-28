using ChessPlatform.Api.Infrastructure.Persistence;
using ChessPlatform.Api.Domain.Entities;
using ChessPlatform.Api.Features.Auth;
using ChessPlatform.Api.Features.Users.Dtos;
using Microsoft.Extensions.Configuration;

namespace ChessPlatform.Api.Tests.Auth;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class AuthServiceTests
{
    private readonly ChessDbContext _db;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<ChessDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new ChessDbContext(options);

        var passwordHasher = new PasswordHasher();

        var jwtService = new JwtTokenService(
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "JwtSettings:Key", "this-is-a-long-test-key-with-more-than-32-chars" },
                    { "JwtSettings:Issuer", "Test" },
                    { "JwtSettings:Audience", "Test" }
                })
                .Build()
        );

        _authService = new AuthService(
            _db,
            passwordHasher,
            jwtService
        );
    }
    
    [Fact]
    public async Task RegisterAsync_WithNewUser_ReturnsSuccess()
    {
        var request = new CreateUserRequest
        {
            Username = "john",
            Email = "john@test.com",
            Password = "password123"
        };
        
        var result = await _authService.RegisterAsync(request);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        result.Value!.Username.Should().Be("john");
        result.Value.PasswordHash.Should().NotBe("password123");
    }
    
    [Fact]
    public async Task RegisterAsync_WhenUsernameExists_ReturnsFailure()
    {
        _db.Users.Add(new User
        {
            Username = "john",
            Email = "old@test.com",
            PasswordHash = "hash"
        });

        await _db.SaveChangesAsync();

        var request = new CreateUserRequest
        {
            Username = "john",
            Email = "new@test.com",
            Password = "password123"
        };
        
        var result = await _authService.RegisterAsync(request);
        
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.UsernameAlreadyInUse);
    }
}