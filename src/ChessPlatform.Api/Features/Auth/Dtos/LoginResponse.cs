namespace ChessPlatform.Api.Features.Auth.Dtos;

public class LoginResponse
{
    public required string Token { get; init; }
    public required DateTime ExpiresAt { get; init; }
}