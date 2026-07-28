using ChessPlatform.Api.Domain.Entities;
using ChessPlatform.Api.Features.Auth.Dtos;

namespace ChessPlatform.Api.Features.Auth;

public interface ITokenService
{
    LoginResponse CreateToken(User user);
}