using FCG.Domain.Entities;
using FCG.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace FCG.Test;

public class DomainEntityTests
{
    [Fact]
    public void Game_Update_ShouldChangeEditableFields()
    {
        var game = new Game("Old", "Old description", 10);

        game.Update("New", "New description", 20);

        game.Title.Should().Be("New");
        game.Description.Should().Be("New description");
        game.Price.Should().Be(20);
    }

    [Fact]
    public void Game_Deactivate_ShouldMarkGameAsInactive()
    {
        var game = new Game("Game", "Description", 10);

        game.Deactivate();

        game.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Promotion_CalculateDiscountAmount_ShouldReturnDiscount_WhenPromotionIsActive()
    {
        var promotion = new Promotion("Promo", Guid.NewGuid(), 10, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

        var discount = promotion.CalculateDiscountAmount(100);

        discount.Should().Be(10);
    }

    [Fact]
    public void Promotion_CalculateDiscountAmount_ShouldReturnZero_WhenPromotionIsInactive()
    {
        var promotion = new Promotion("Promo", Guid.NewGuid(), 10, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));
        promotion.Deactivate();

        var discount = promotion.CalculateDiscountAmount(100);

        discount.Should().Be(0);
    }

    [Fact]
    public void Promotion_CalculateDiscountAmount_ShouldReturnZero_WhenPromotionIsExpired()
    {
        var promotion = new Promotion("Promo", Guid.NewGuid(), 10, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));

        var discount = promotion.CalculateDiscountAmount(100);

        discount.Should().Be(0);
    }

    [Fact]
    public void Promotion_Activate_ShouldMarkPromotionAsActive()
    {
        var promotion = new Promotion("Promo", Guid.NewGuid(), 10, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));
        promotion.Deactivate();

        promotion.Activate();

        promotion.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Promotion_Update_ShouldChangeEditableFields()
    {
        var promotion = new Promotion("Old", Guid.NewGuid(), 10, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        var startDate = DateTime.UtcNow.AddDays(2);
        var endDate = DateTime.UtcNow.AddDays(3);

        promotion.Update("New", 20, startDate, endDate);

        promotion.Name.Should().Be("New");
        promotion.DiscountPercentage.Should().Be(20);
        promotion.StartDate.Should().Be(startDate);
        promotion.EndDate.Should().Be(endDate);
    }

    [Fact]
    public void User_AddGameToLibrary_ShouldNotDuplicateGame()
    {
        var user = new User("User", "user@email.com", "hash", Role.User);
        var game = new Game("Game", "Description", 10);

        user.AddGameToLibrary(game);
        user.AddGameToLibrary(game);

        user.LibraryItems.Should().ContainSingle(libraryItem => libraryItem.GameId == game.Id);
    }

    [Fact]
    public void User_ChangeRole_ShouldUpdateRole()
    {
        var user = new User("User", "user@email.com", "hash", Role.User);

        user.ChangeRole(Role.Admin);

        user.Role.Should().Be(Role.Admin);
    }

    [Fact]
    public void LibraryItem_ShouldExposeUserGameAndAcquiredAt()
    {
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        var libraryItem = new LibraryItem(userId, gameId);

        libraryItem.Id.Should().NotBeEmpty();
        libraryItem.UserId.Should().Be(userId);
        libraryItem.GameId.Should().Be(gameId);
        libraryItem.AcquiredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void OrderItem_ShouldExposeGameAndPrice()
    {
        var gameId = Guid.NewGuid();

        var orderItem = new OrderItem(gameId, 25);

        orderItem.Id.Should().NotBeEmpty();
        orderItem.GameId.Should().Be(gameId);
        orderItem.PriceAtPurchase.Should().Be(25);
    }
}
