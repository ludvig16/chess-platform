using System.ComponentModel.DataAnnotations;

namespace ChessPlatform.Users;

public class User
{
    [Key]
    public int Id { get; private set; }

    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required string Email { get; set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; set; }
}