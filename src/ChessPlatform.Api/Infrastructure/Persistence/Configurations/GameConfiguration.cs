using ChessPlatform.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChessPlatform.Api.Infrastructure.Persistence.Configurations;

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Status).HasConversion<string>();
        builder.Property(g => g.SideToMove).HasConversion<string>();
        builder.Property(g => g.Termination).HasConversion<string>();
        builder.Property(g => g.Winner).HasConversion<string>();

        builder.HasMany(g => g.Moves)
            .WithOne(m => m.Game)
            .HasForeignKey(m => m.GameId);
    }
}