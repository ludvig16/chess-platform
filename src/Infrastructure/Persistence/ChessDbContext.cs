using ChessPlatform.Domain.Entities;
using ChessPlatform.Features.Users;
using Microsoft.EntityFrameworkCore;

namespace ChessPlatform.Infrastructure.Persistence;

public class ChessDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public ChessDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration["DefaultConnection"]);
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChessDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
    
    public DbSet<User> Users { get; set; }
}