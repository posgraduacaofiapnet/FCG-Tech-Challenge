using FCG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Infrastructure.Configurations;

internal class LibraryItemMap() : IEntityTypeConfiguration<LibraryItem>
{
    public void Configure(EntityTypeBuilder<LibraryItem> builder)
    {
        builder.HasKey(libraryItem => libraryItem.Id);
        builder.Property(libraryItem => libraryItem.Id).ValueGeneratedNever();

        builder.HasIndex(libraryItem => new { libraryItem.UserId, libraryItem.GameId }).IsUnique();

        builder.HasOne(libraryItem => libraryItem.User)
            .WithMany(user => user.LibraryItems)
            .HasForeignKey(libraryItem => libraryItem.UserId);

        builder.HasOne(libraryItem => libraryItem.Game)
            .WithMany(game => game.LibraryItems)
            .HasForeignKey(libraryItem => libraryItem.GameId);
    }
}
