namespace ChessPlatform.Features.Users.Dtos;

public class UserResponse
{
    public required string Username { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}