namespace ChessPlatform.Api.Features.Users.Dtos;

public class UserResponse
{
    public required int Id { get; set;}
    public required string Username { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}