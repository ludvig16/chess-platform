using ChessPlatform.Domain.Entities;
using ChessPlatform.Features.Users.Dtos;

namespace ChessPlatform.Features.Users;

public static class UserMappers
{
    public static UserResponse ToUserResponseDto(this User user)
    {
        return new UserResponse
        {
            Username = user.Username,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
    
    public static User ToUserFromCreateDto(this CreateUserRequest userRequest)
    {
        return new User
        {
            Username = userRequest.Username,
            PasswordHash = userRequest.Password,
            Email = userRequest.Email
        };
    }
}