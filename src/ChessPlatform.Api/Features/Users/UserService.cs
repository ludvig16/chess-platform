using ChessPlatform.Api.Domain.Entities;
using ChessPlatform.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ChessPlatform.Api.Features.Users.Dtos;

namespace ChessPlatform.Api.Features.Users;

public class UserService
{
    private readonly ChessDbContext _db;
    
    public UserService(ChessDbContext db)
    {
        _db = db;
    }
    
    public async Task<User> CreateUser(CreateUserRequest request)
    {
        var user = request.ToUserFromCreateDto();
        
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.PasswordHash = hashedPassword;

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return user;
    }
    
    public Task<List<User>> GetAllAsync()
    {
        return _db.Users.ToListAsync();
    }

    public ValueTask<User?> GetUserByIdAsync(int id)
    {
        return _db.Users.FindAsync(id);
    }

    public Task<User?> GetUserByEmailAsync(string email)
    {
        return _db.Users.SingleOrDefaultAsync(u => u.Email == email);
    }

    public Task<User?> GetUserByUsernameOrEmailAsync(string username, string email)
    {
        return _db.Users.SingleOrDefaultAsync(u => u.Email == email || u.Username == username);
    }
}