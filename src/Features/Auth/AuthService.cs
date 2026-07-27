using ChessPlatform.Domain.Entities;
using ChessPlatform.Features.Auth.Dtos;
using ChessPlatform.Features.Users;
using ChessPlatform.Features.Users.Dtos;
using ChessPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using LoginRequest = ChessPlatform.Features.Auth.Dtos.LoginRequest;
using ChessPlatform.Features.Common;

namespace ChessPlatform.Features.Auth;

public class AuthService
{
    private readonly ChessDbContext _db;
    private readonly PasswordHasher _passwordHasher;
    private readonly ITokenService _jwtTokenService;
    
    public AuthService(ChessDbContext dbContext, PasswordHasher passwordHasher, ITokenService jwtTokenService)
    {
        _db = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<User>> RegisterAsync(CreateUserRequest request)
    {
        var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);

        if (existingUser is not null)
        {
            if (existingUser.Username == request.Username)
                return Result<User>.Failure(AuthErrors.UsernameAlreadyInUse);

            if (existingUser.Email == request.Email)
                return Result<User>.Failure(AuthErrors.EmailAlreadyInUse);
        }

        var user = request.ToUserFromCreateDto();
        user.PasswordHash = _passwordHasher.HashPassword(user.PasswordHash);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Result<User>.Success(user);
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user is null)
        {
            return Result<LoginResponse>.Failure(AuthErrors.UserNotFound);
        }

        var valid = _passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!valid)
        {
            return Result<LoginResponse>.Failure(AuthErrors.InvalidCredentials);
        }

        var token = _jwtTokenService.CreateToken(user);

        return Result<LoginResponse>.Success(token);
    }
}