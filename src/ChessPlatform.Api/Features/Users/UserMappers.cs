using ChessPlatform.Api.Domain.Entities;
using ChessPlatform.Api.Features.Users.Dtos;

namespace ChessPlatform.Api.Features.Users;

public static class UserMappers
{
    public static UserResponse ToUserResponseDto(this User user)
    {
        return new UserResponse
        {
            Id = user.Id,
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