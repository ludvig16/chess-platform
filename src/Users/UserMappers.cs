using ChessPlatform.Users.Dtos;

namespace ChessPlatform.Users;

public static class UserMappers
{
    public static UserResponseDto ToUserResponseDto(this User user)
    {
        return new UserResponseDto
        {
            Username = user.Username,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
    
    public static User ToUserFromCreateDto(this CreateUserRequestDto userRequestDto)
    {
        return new User
        {
            Username = userRequestDto.Username,
            PasswordHash = userRequestDto.Password,
            Email = userRequestDto.Email
        };
    }
}