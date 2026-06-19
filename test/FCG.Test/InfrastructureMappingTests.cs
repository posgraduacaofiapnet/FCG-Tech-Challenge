using FCG.Domain.Entities;
using FCG.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace FCG.Test;

public class InfrastructureMappingTests
{
    [Theory]
    [InlineData(typeof(Game))]
    [InlineData(typeof(LibraryItem))]
    [InlineData(typeof(Order))]
    [InlineData(typeof(OrderItem))]
    [InlineData(typeof(Promotion))]
    [InlineData(typeof(User))]
    public void EntityIds_ShouldBeGeneratedByApplication(Type entityType)
    {
        using var context = CreateContext();

        var property = context.Model
            .FindEntityType(entityType)!
            .FindProperty(nameof(Entity.Id))!;

        property.ValueGenerated.Should().Be(ValueGenerated.Never);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
