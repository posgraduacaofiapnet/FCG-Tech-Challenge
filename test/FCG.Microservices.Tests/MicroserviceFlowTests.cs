using CatalogAPI;
using FCG.Contracts;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NotificationsAPI;
using PaymentsAPI;
using UsersAPI;

namespace FCG.Microservices.Tests;

public class MicroserviceFlowTests
{
    [Fact]
    public void PasswordPolicy_ShouldRejectWeakPassword()
    {
        PasswordPolicy.IsStrong("weak").Should().BeFalse();
        PasswordPolicy.IsStrong("Senha@123").Should().BeTrue();
    }

    [Fact]
    public async Task AuthService_RegisterAsync_ShouldPublishUserCreatedEvent()
    {
        await using var dbContext = new UsersDbContext(new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        var publisher = new FakeUserPublisher();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "zR8pW5vB9yX2mN4qA7L1jK9sT6uE3hG0fD5cX8vB2nN1mQ4wZ7xR0tY3uI6pP9oL",
                ["Jwt:Issuer"] = "UsersAPI",
                ["Jwt:Audience"] = "FCG"
            })
            .Build();

        var service = new AuthService(dbContext, new JwtTokenService(configuration), publisher);

        var result = await service.RegisterAsync(new RegisterUserRequest("User", "user@email.com", "Senha@123"), CancellationToken.None);

        result.Should().NotBeNull();
        publisher.Published.Should().ContainSingle();
        publisher.Published[0].Email.Should().Be("user@email.com");
    }

    [Fact]
    public async Task CatalogService_PurchaseAsync_ShouldPersistOrderAndPublishOrderPlacedEvent()
    {
        await using var dbContext = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        var publisher = new FakeCatalogPublisher();
        var service = new CatalogService(dbContext, publisher);
        var game = await service.CreateGameAsync(new CreateGameRequest("Game", "Description", 99.90m), CancellationToken.None);

        await service.PurchaseAsync(new PurchaseGameRequest(Guid.NewGuid(), game.Id), CancellationToken.None);

        dbContext.Orders.Should().ContainSingle(order => order.GameId == game.Id && order.Status == "Pending");
        publisher.Published.Should().ContainSingle();
        publisher.Published[0].GameTitle.Should().Be("Game");
    }

    [Fact]
    public void PaymentProcessor_ShouldApproveByDefault()
    {
        var processor = new PaymentProcessor(new ConfigurationBuilder().Build());
        var order = new OrderPlacedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Game", 10, DateTime.UtcNow);

        var payment = processor.Process(order);

        payment.Status.Should().Be(PaymentStatuses.Approved);
        payment.OrderId.Should().Be(order.OrderId);
    }

    [Fact]
    public void NotificationLogService_ShouldReturnPurchaseConfirmationOnlyForApprovedPayment()
    {
        var service = new NotificationLogService(NullLogger<NotificationLogService>.Instance);
        var approved = new PaymentProcessedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Game", 10, PaymentStatuses.Approved, DateTime.UtcNow);
        var rejected = approved with { Status = PaymentStatuses.Rejected };

        service.SendPurchaseConfirmation(approved).Should().Contain("confirmacao de compra");
        service.SendPurchaseConfirmation(rejected).Should().BeNull();
    }

    private sealed class FakeUserPublisher : IUserEventPublisher
    {
        public List<UserCreatedEvent> Published { get; } = [];

        public Task PublishUserCreatedAsync(UserCreatedEvent message, CancellationToken cancellationToken)
        {
            Published.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeCatalogPublisher : ICatalogEventPublisher
    {
        public List<OrderPlacedEvent> Published { get; } = [];

        public Task PublishOrderPlacedAsync(OrderPlacedEvent message, CancellationToken cancellationToken)
        {
            Published.Add(message);
            return Task.CompletedTask;
        }
    }
}
