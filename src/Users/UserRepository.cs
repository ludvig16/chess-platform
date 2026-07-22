using ChessPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChessPlatform.Users;

public class UserRepository
{
    private readonly ChessDbContext _db;

    public UserRepository(ChessDbContext dbContext)
    {
        _db = dbContext;
    }

    public Task<List<User>> GetAllAsync()
    {
        return _db.Users.ToListAsync();
    }

    public ValueTask<User?> GetUserByIdAsync(int id)
    {
        return _db.Users.FindAsync(id);
    }
}