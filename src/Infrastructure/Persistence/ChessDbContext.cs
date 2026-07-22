using ChessPlatform.Users;
using Microsoft.EntityFrameworkCore;

namespace ChessPlatform.Infrastructure.Persistence;

public class ChessDbContext : DbContext
{
    protected readonly IConfiguration Configuration;

    public ChessDbContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(Environment.GetEnvironmentVariable("DefaultConnection"));
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChessDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
    
    public DbSet<User> Users { get; set; }
}