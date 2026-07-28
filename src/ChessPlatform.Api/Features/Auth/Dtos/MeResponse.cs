namespace ChessPlatform.Api.Features.Auth.Dtos;

public class MeResponse
{
    public int Id { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
}