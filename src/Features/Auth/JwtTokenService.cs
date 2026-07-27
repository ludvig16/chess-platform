using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChessPlatform.Domain.Entities;
using ChessPlatform.Features.Auth.Dtos;
using Microsoft.IdentityModel.Tokens;

namespace ChessPlatform.Features.Auth;

public class JwtTokenService : ITokenService
{

    public LoginResponse CreateToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JwtSettings__Key")!));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddHours(1);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var token = new JwtSecurityToken(
            issuer: Environment.GetEnvironmentVariable("JwtSettings__Issuer"),
            audience: Environment.GetEnvironmentVariable("JwtSettings__Audience"),
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expires
        };
    }
}