using FCG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Infrastructure.Configurations;

public class OrderMap() : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(order => order.Id);
        builder.Property(order => order.Id).ValueGeneratedNever();
        builder.Property(order => order.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(order => order.CreatedAt).IsRequired();
        builder.Property(order => order.Status).IsRequired();

        builder.HasOne(order => order.User)
            .WithMany()
            .HasForeignKey(order => order.UserId);

        builder.HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey(orderItem => orderItem.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
