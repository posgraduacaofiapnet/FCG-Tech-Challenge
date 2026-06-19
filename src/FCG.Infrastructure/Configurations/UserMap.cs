using FCG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Infrastructure.Configurations;

public class UserMap() : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedNever();
        builder.Property(user => user.Name).IsRequired().HasMaxLength(150);
        builder.Property(user => user.Email).IsRequired().HasMaxLength(150);
        builder.HasIndex(user => user.Email).IsUnique();
        builder.Property(user => user.PasswordHash).IsRequired();
        builder.Property(user => user.Role).IsRequired();
        builder.Property(user => user.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(user => user.DeactivatedAt);
        builder.Property(user => user.ReactivatedAt);
    }
}
