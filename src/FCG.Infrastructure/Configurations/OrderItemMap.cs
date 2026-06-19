using FCG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Infrastructure.Configurations;

public class OrderItemMap() : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(orderItem => orderItem.Id);
        builder.Property(orderItem => orderItem.Id).ValueGeneratedNever();
        builder.Property(orderItem => orderItem.PriceAtPurchase).HasColumnType("decimal(18,2)");

        builder.HasOne(orderItem => orderItem.Game)
            .WithMany()
            .HasForeignKey(orderItem => orderItem.GameId);
    }
}
