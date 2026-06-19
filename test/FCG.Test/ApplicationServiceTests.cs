using FCG.Application.DTOs;
using FCG.Application.Services;
using FCG.Application.Settings;
using FCG.Domain.Common;
using FCG.Domain.Entities;
using FCG.Domain.Enums;
using FCG.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace FCG.Test;

public class ApplicationServiceTests
{
    [Fact]
    public async Task GameService_GetAllAsync_ShouldFail_WhenPageIsInvalid()
    {
        var service = CreateGameService();

        var result = await service.GetAllAsync(0);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Pagination.InvalidPage);
    }

    [Fact]
    public async Task GameService_CreateAsync_ShouldCreateGame()
    {
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateGameService(unitOfWork: unitOfWork);

        var result = await service.CreateAsync(new CreateGameDto("Game", "Description", 10));

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Game");
        unitOfWork.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task GameService_GetByIdAsync_ShouldReturnGame_WhenGameExists()
    {
        var game = new Game("Game", "Description", 10);
        var service = CreateGameService(game);

        var result = await service.GetByIdAsync(game.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(game.Id);
    }

    [Fact]
    public async Task GameService_GetAllAsync_ShouldReturnPagedGames()
    {
        var service = CreateGameService(new Game("Game", "Description", 10));

        var result = await service.GetAllAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task GameService_AddToLibraryAsync_ShouldCommit_WhenUserAndGameExist()
    {
        var unitOfWork = new FakeUnitOfWork();
        var game = new Game("Game", "Description", 10);
        var service = CreateGameService(game, unitOfWork);

        var result = await service.AddToLibraryAsync(Guid.NewGuid(), game.Id);

        result.IsSuccess.Should().BeTrue();
        unitOfWork.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task GameService_GetLibraryAsync_ShouldReturnUserLibrary()
    {
        var service = CreateGameService();

        var result = await service.GetLibraryAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task PromotionService_CreateAsync_ShouldCreatePromotion_WhenGameExists()
    {
        var game = new Game("Game", "Description", 100);
        var promotionRepository = new FakePromotionRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new PromotionService(
            promotionRepository,
            new FakeGameRepository(game),
            unitOfWork,
            Options.Create(new PaginationSettings { PageSize = 30 }));

        var result = await service.CreateAsync(new CreatePromotionDto("Promo", game.Id, 10, DateTime.UtcNow, DateTime.UtcNow.AddDays(1)));

        result.IsSuccess.Should().BeTrue();
        promotionRepository.Promotions.Should().ContainSingle();
        unitOfWork.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task PromotionService_DeactivateAsync_ShouldDeactivatePromotion()
    {
        var game = new Game("Game", "Description", 100);
        var promotion = new Promotion("Promo", game.Id, 10, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        var unitOfWork = new FakeUnitOfWork();
        var service = new PromotionService(
            new FakePromotionRepository(promotion),
            new FakeGameRepository(game),
            unitOfWork,
            Options.Create(new PaginationSettings { PageSize = 30 }));

        var result = await service.DeactivateAsync(promotion.Id);

        result.IsSuccess.Should().BeTrue();
        promotion.IsActive.Should().BeFalse();
        unitOfWork.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task PromotionService_GetAllActiveAsync_ShouldReturnActivePromotions()
    {
        var game = new Game("Game", "Description", 100);
        var promotion = new Promotion("Promo", game.Id, 10, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        typeof(Promotion).GetProperty(nameof(Promotion.Game))!.SetValue(promotion, game);
        var service = new PromotionService(
            new FakePromotionRepository(promotion),
            new FakeGameRepository(game),
            new FakeUnitOfWork(),
            Options.Create(new PaginationSettings { PageSize = 30 }));

        var result = await service.GetAllActiveAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task PromotionService_GetAllActiveAsync_ShouldFail_WhenPageIsInvalid()
    {
        var service = new PromotionService(
            new FakePromotionRepository(),
            new FakeGameRepository(),
            new FakeUnitOfWork(),
            Options.Create(new PaginationSettings { PageSize = 30 }));

        var result = await service.GetAllActiveAsync(0);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Pagination.InvalidPage);
    }

    [Fact]
    public async Task OrderService_CreateOrderAsync_ShouldApplyBestPromotion()
    {
        var game = new Game("Game", "Description", 100);
        var promotion = new Promotion("Promo", game.Id, 25, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));
        var orderRepository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new OrderService(
            new FakeGameRepository(game),
            orderRepository,
            new FakeUserRepository(),
            new FakePromotionRepository(promotion),
            unitOfWork);

        var result = await service.CreateOrderAsync(Guid.NewGuid(), new CreateOrderDto([game.Id]));

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalAmount.Should().Be(75);
        orderRepository.Orders.Should().ContainSingle();
        unitOfWork.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task OrderService_CreateOrderAsync_ShouldFail_WhenGameListIsEmpty()
    {
        var service = new OrderService(
            new FakeGameRepository(),
            new FakeOrderRepository(),
            new FakeUserRepository(),
            new FakePromotionRepository(),
            new FakeUnitOfWork());

        var result = await service.CreateOrderAsync(Guid.NewGuid(), new CreateOrderDto([]));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Orders.EmptyGames);
    }

    [Fact]
    public async Task OrderService_GetUserOrdersAsync_ShouldReturnOnlyUserOrders()
    {
        var userId = Guid.NewGuid();
        var order = new Order(userId);
        var service = new OrderService(
            new FakeGameRepository(),
            new FakeOrderRepository(order),
            new FakeUserRepository(),
            new FakePromotionRepository(),
            new FakeUnitOfWork());

        var result = await service.GetUserOrdersAsync(userId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(orderDto => orderDto.Id == order.Id);
    }

    private static GameService CreateGameService(Game? game = null, FakeUnitOfWork? unitOfWork = null)
    {
        return new GameService(
            new FakeGameRepository(game),
            new FakeUserRepository(),
            unitOfWork ?? new FakeUnitOfWork(),
            Options.Create(new PaginationSettings { PageSize = 30 }));
    }

    private sealed class FakeGameRepository(Game? game = null) : IGameRepository
    {
        private readonly List<Game> _games = game is null ? [] : [game];

        public Task<Result> AddAsync(Game entity)
        {
            _games.Add(entity);
            return Task.FromResult(Result.Success());
        }

        public Task<Result<PagedResult<Game>>> GetAllAsync(PaginationParameters pagination)
        {
            return Task.FromResult(Result<PagedResult<Game>>.Success(new PagedResult<Game>(_games, pagination.Page, pagination.PageSize, _games.Count)));
        }

        public Task<Result<Game>> GetByIdAsync(Guid id)
        {
            var foundGame = _games.FirstOrDefault(game => game.Id == id);
            return Task.FromResult(foundGame is null
                ? Result<Game>.Failure(Errors.Games.NotFound)
                : Result<Game>.Success(foundGame));
        }

        public Task<Result> UpdateAsync(Game entity) => Task.FromResult(Result.Success());
        public Task<Result> DeleteAsync(Game entity) => Task.FromResult(Result.Success());
    }

    private sealed class FakePromotionRepository(params Promotion[] promotions) : IPromotionRepository
    {
        public List<Promotion> Promotions { get; } = promotions.ToList();

        public Task<Result> AddAsync(Promotion entity)
        {
            Promotions.Add(entity);
            return Task.FromResult(Result.Success());
        }

        public Task<Result<PagedResult<Promotion>>> GetAllActiveAsync(PaginationParameters pagination)
        {
            return Task.FromResult(Result<PagedResult<Promotion>>.Success(new PagedResult<Promotion>(Promotions.Where(promotion => promotion.IsActive).ToArray(), pagination.Page, pagination.PageSize, Promotions.Count)));
        }

        public Task<Result<PagedResult<Promotion>>> GetAllAsync(PaginationParameters pagination)
        {
            return Task.FromResult(Result<PagedResult<Promotion>>.Success(new PagedResult<Promotion>(Promotions, pagination.Page, pagination.PageSize, Promotions.Count)));
        }

        public Task<Result<IEnumerable<Promotion>>> GetActivePromotionsByGameIdAsync(Guid gameId)
        {
            return Task.FromResult(Result<IEnumerable<Promotion>>.Success(Promotions.Where(promotion => promotion.GameId == gameId && promotion.IsActive)));
        }

        public Task<Result<Promotion>> GetByIdAsync(Guid id)
        {
            var promotion = Promotions.FirstOrDefault(promotion => promotion.Id == id);
            return Task.FromResult(promotion is null
                ? Result<Promotion>.Failure(Errors.Promotions.NotFound)
                : Result<Promotion>.Success(promotion));
        }

        public Task<Result> UpdateAsync(Promotion entity) => Task.FromResult(Result.Success());
        public Task<Result> DeleteAsync(Promotion entity) => Task.FromResult(Result.Success());
    }

    private sealed class FakeOrderRepository(params Order[] orders) : IOrderRepository
    {
        public List<Order> Orders { get; } = orders.ToList();

        public Task<Result> AddAsync(Order entity)
        {
            Orders.Add(entity);
            return Task.FromResult(Result.Success());
        }

        public Task<Result<IEnumerable<Order>>> GetByUserIdAsync(Guid userId)
        {
            return Task.FromResult(Result<IEnumerable<Order>>.Success(Orders.Where(order => order.UserId == userId)));
        }

        public Task<Result<Order>> GetByIdAsync(Guid id)
        {
            var order = Orders.FirstOrDefault(order => order.Id == id);
            return Task.FromResult(order is null
                ? Result<Order>.Failure(Errors.Orders.NotFound)
                : Result<Order>.Success(order));
        }

        public Task<Result> UpdateAsync(Order entity) => Task.FromResult(Result.Success());
        public Task<Result> DeleteAsync(Order entity) => Task.FromResult(Result.Success());
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly User _user = new("User", "user@email.com", "hash", Role.User);

        public Task<Result> AddAsync(User entity) => Task.FromResult(Result.Success());
        public Task<Result<PagedResult<User>>> GetAllAsync(PaginationParameters pagination) => Task.FromResult(Result<PagedResult<User>>.Success(new PagedResult<User>([], pagination.Page, pagination.PageSize, 0)));
        public Task<Result<User>> GetByEmailAsync(string email) => Task.FromResult(Result<User>.Failure(Errors.Users.NotFoundByEmail));
        public Task<Result<User>> GetByIdAsync(Guid id) => Task.FromResult(Result<User>.Success(_user));
        public Task<Result<User>> GetByIdIncludingInactiveAsync(Guid id) => Task.FromResult(Result<User>.Success(_user));
        public Task<Result<PagedResult<User>>> GetAllIncludingInactiveAsync(PaginationParameters pagination) => GetAllAsync(pagination);
        public Task<Result<User>> GetByEmailIncludingInactiveAsync(string email) => Task.FromResult(Result<User>.Failure(Errors.Users.NotFoundByEmail));
        public Task<Result> UpdateAsync(User entity) => Task.FromResult(Result.Success());
        public Task<Result> DeleteAsync(User entity) => Task.FromResult(Result.Success());
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int CommitCount { get; private set; }

        public Task<Result> CommitAsync(CancellationToken cancellationToken = default)
        {
            CommitCount++;
            return Task.FromResult(Result.Success());
        }
    }
}
