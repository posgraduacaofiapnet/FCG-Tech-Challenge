using FCG.Application.DTOs;
using FCG.Application.Services;
using FCG.Domain.Common;
using FCG.Domain.Entities;
using FCG.Domain.Enums;
using FCG.Domain.Interfaces;
using FluentAssertions;
using Xunit;

namespace FCG.Test;

public class OrderServiceAuthorizationTests
{
    [Fact]
    public async Task GetOrderByIdAsync_ShouldFail_WhenUserDoesNotOwnOrder()
    {
        var ownerId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var order = new Order(ownerId);
        var service = CreateService(order);

        var result = await service.GetOrderByIdAsync(order.Id, currentUserId, isAdmin: false);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Orders.AccessDenied);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ShouldReturnOrder_WhenUserIsAdmin()
    {
        var order = new Order(Guid.NewGuid());
        var service = CreateService(order);

        var result = await service.GetOrderByIdAsync(order.Id, Guid.NewGuid(), isAdmin: true);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task ApprovePaymentAsync_ShouldFail_WhenUserDoesNotOwnOrder()
    {
        var order = new Order(Guid.NewGuid());
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(order, unitOfWork);

        var result = await service.ApprovePaymentAsync(order.Id, Guid.NewGuid(), isAdmin: false);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Orders.AccessDenied);
        unitOfWork.CommitCount.Should().Be(0);
    }

    [Fact]
    public async Task ApprovePaymentAsync_ShouldApproveOrder_WhenUserOwnsOrder()
    {
        var ownerId = Guid.NewGuid();
        var order = new Order(ownerId);
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(order, unitOfWork);

        var result = await service.ApprovePaymentAsync(order.Id, ownerId, isAdmin: false);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Paid);
        unitOfWork.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task ApprovePaymentAsync_ShouldCommitTrackedChanges_WithoutCallingRepositoryUpdate()
    {
        var ownerId = Guid.NewGuid();
        var order = new Order(ownerId);
        var user = new User("Owner", "owner@email.com", "hash", Role.User);
        var orderRepository = new FakeOrderRepository(order);
        var userRepository = new FakeUserRepository(user);
        var unitOfWork = new FakeUnitOfWork();
        var service = new OrderService(
            new FakeGameRepository(),
            orderRepository,
            userRepository,
            new FakePromotionRepository(),
            unitOfWork);

        var result = await service.ApprovePaymentAsync(order.Id, ownerId, isAdmin: false);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Paid);
        orderRepository.UpdateCount.Should().Be(0);
        userRepository.UpdateCount.Should().Be(0);
        unitOfWork.CommitCount.Should().Be(1);
    }

    private static OrderService CreateService(Order order, FakeUnitOfWork? unitOfWork = null)
    {
        return new OrderService(
            new FakeGameRepository(),
            new FakeOrderRepository(order),
            new FakeUserRepository(new User("Owner", "owner@email.com", "hash", Role.User)),
            new FakePromotionRepository(),
            unitOfWork ?? new FakeUnitOfWork());
    }

    private sealed class FakeOrderRepository(Order order) : IOrderRepository
    {
        public int UpdateCount { get; private set; }

        public Task<Result> AddAsync(Order entity) => Task.FromResult(Result.Success());
        public Task<Result<IEnumerable<Order>>> GetByUserIdAsync(Guid userId) => Task.FromResult(Result<IEnumerable<Order>>.Success([order]));
        public Task<Result<Order>> GetByIdAsync(Guid id) => Task.FromResult(id == order.Id ? Result<Order>.Success(order) : Result<Order>.Failure(Errors.Orders.NotFound));
        public Task<Result> UpdateAsync(Order entity)
        {
            UpdateCount++;
            return Task.FromResult(Result.Success());
        }

        public Task<Result> DeleteAsync(Order entity) => Task.FromResult(Result.Success());
    }

    private sealed class FakeUserRepository(User user) : IUserRepository
    {
        public int UpdateCount { get; private set; }

        public Task<Result> AddAsync(User entity) => Task.FromResult(Result.Success());
        public Task<Result<PagedResult<User>>> GetAllAsync(PaginationParameters pagination) => Task.FromResult(Result<PagedResult<User>>.Success(new PagedResult<User>([user], 1, 30, 1)));
        public Task<Result<User>> GetByEmailAsync(string email) => Task.FromResult(Result<User>.Success(user));
        public Task<Result<User>> GetByIdAsync(Guid id) => Task.FromResult(Result<User>.Success(user));
        public Task<Result<User>> GetByIdIncludingInactiveAsync(Guid id) => Task.FromResult(Result<User>.Success(user));
        public Task<Result<PagedResult<User>>> GetAllIncludingInactiveAsync(PaginationParameters pagination) => GetAllAsync(pagination);
        public Task<Result<User>> GetByEmailIncludingInactiveAsync(string email) => Task.FromResult(Result<User>.Success(user));
        public Task<Result> UpdateAsync(User entity)
        {
            UpdateCount++;
            return Task.FromResult(Result.Success());
        }

        public Task<Result> DeleteAsync(User entity) => Task.FromResult(Result.Success());
    }

    private sealed class FakeGameRepository : IGameRepository
    {
        public Task<Result> AddAsync(Game entity) => Task.FromResult(Result.Success());
        public Task<Result<PagedResult<Game>>> GetAllAsync(PaginationParameters pagination) => Task.FromResult(Result<PagedResult<Game>>.Success(new PagedResult<Game>([], 1, 30, 0)));
        public Task<Result<Game>> GetByIdAsync(Guid id) => Task.FromResult(Result<Game>.Failure(Errors.Games.NotFound));
        public Task<Result> UpdateAsync(Game entity) => Task.FromResult(Result.Success());
        public Task<Result> DeleteAsync(Game entity) => Task.FromResult(Result.Success());
    }

    private sealed class FakePromotionRepository : IPromotionRepository
    {
        public Task<Result> AddAsync(Promotion entity) => Task.FromResult(Result.Success());
        public Task<Result<PagedResult<Promotion>>> GetAllActiveAsync(PaginationParameters pagination) => Task.FromResult(Result<PagedResult<Promotion>>.Success(new PagedResult<Promotion>([], 1, 30, 0)));
        public Task<Result<PagedResult<Promotion>>> GetAllAsync(PaginationParameters pagination) => Task.FromResult(Result<PagedResult<Promotion>>.Success(new PagedResult<Promotion>([], 1, 30, 0)));
        public Task<Result<IEnumerable<Promotion>>> GetActivePromotionsByGameIdAsync(Guid gameId) => Task.FromResult(Result<IEnumerable<Promotion>>.Success([]));
        public Task<Result<Promotion>> GetByIdAsync(Guid id) => Task.FromResult(Result<Promotion>.Failure(Errors.Promotions.NotFound));
        public Task<Result> UpdateAsync(Promotion entity) => Task.FromResult(Result.Success());
        public Task<Result> DeleteAsync(Promotion entity) => Task.FromResult(Result.Success());
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
