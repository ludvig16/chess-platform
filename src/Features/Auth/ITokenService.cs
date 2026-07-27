using ChessPlatform.Domain.Entities;
using ChessPlatform.Features.Auth.Dtos;

namespace ChessPlatform.Features.Auth;

public interface ITokenService
{
    LoginResponse CreateToken(User user);
}