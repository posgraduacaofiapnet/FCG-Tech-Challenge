using FCG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Infrastructure.Configurations;

internal class PromotionMap() : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {

        builder.HasKey(promotion => promotion.Id);
        builder.Property(promotion => promotion.Id).ValueGeneratedNever();
        builder.Property(promotion => promotion.Name).IsRequired().HasMaxLength(150);
        builder.Property(promotion => promotion.DiscountPercentage).HasColumnType("decimal(5,2)");
        builder.Property(promotion => promotion.StartDate).IsRequired();
        builder.Property(promotion => promotion.EndDate).IsRequired();
        builder.Property(promotion => promotion.IsActive).HasDefaultValue(true);
        builder.HasOne(promotion => promotion.Game)
         .WithMany(game => game.Promotions)
         .HasForeignKey(promotion => promotion.GameId);
    }
}
