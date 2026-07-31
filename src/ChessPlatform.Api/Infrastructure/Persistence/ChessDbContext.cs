using ChessPlatform.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChessPlatform.Api.Infrastructure.Persistence;

public class ChessDbContext : DbContext
{
    public ChessDbContext(
        DbContextOptions<ChessDbContext> options)
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChessDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Move> Moves { get; set; }
    public DbSet<Game> Games { get; set; }
}