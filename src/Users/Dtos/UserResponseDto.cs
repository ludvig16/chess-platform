namespace ChessPlatform.Users.Dtos;

public class UserResponseDto
{
    public required string Username { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}