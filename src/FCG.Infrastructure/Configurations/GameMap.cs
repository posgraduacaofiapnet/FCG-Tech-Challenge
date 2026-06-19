using FCG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Infrastructure.Configurations;

public class GameMap() : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.HasKey(game => game.Id);
        builder.Property(game => game.Id).ValueGeneratedNever();
        builder.Property(game => game.Title).IsRequired().HasMaxLength(150);
        builder.Property(game => game.Description).HasMaxLength(500);
        builder.Property(game => game.Price).HasColumnType("decimal(18,2)");
    }
}
