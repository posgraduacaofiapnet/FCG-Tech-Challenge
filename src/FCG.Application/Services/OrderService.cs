using FCG.Application.DTOs;
using FCG.Application.Interfaces;
using FCG.Domain.Common;
using FCG.Domain.Entities;
using FCG.Domain.Interfaces;

namespace FCG.Application.Services;

public sealed class OrderService(
    IGameRepository gameRepository,
    IOrderRepository orderRepository,
    IUserRepository userRepository,
    IPromotionRepository promotionRepository,
    IUnitOfWork unitOfWork) : IOrderService
{
    public async Task<Result> ApprovePaymentAsync(Guid orderId, Guid currentUserId, bool isAdmin)
    {
        var orderResult = await orderRepository.GetByIdAsync(orderId);
        if (orderResult.IsFailure)
        {
            return Result.Failure(orderResult.Error!);
        }

        var order = orderResult.Value;
        if (!CanAccessOrder(order, currentUserId, isAdmin))
        {
            return Result.Failure(Errors.Orders.AccessDenied);
        }

        var userResult = await userRepository.GetByIdAsync(order.UserId);
        if (userResult.IsFailure)
        {
            return Result.Failure(userResult.Error!);
        }

        var markAsPaidResult = order.MarkAsPaid();
        if (markAsPaidResult.IsFailure)
        {
            return markAsPaidResult;
        }

        foreach (var item in order.Items)
        {
            userResult.Value.AddGameToLibrary(item.Game);
        }

        return await unitOfWork.CommitAsync();
    }

    public async Task<Result<OrderDto>> CreateOrderAsync(Guid userId, CreateOrderDto dto)
    {
        if (dto.GameIds.Count == 0)
        {
            return Result<OrderDto>.Failure(Errors.Orders.EmptyGames);
        }

        var order = new Order(userId);

        foreach (var gameId in dto.GameIds)
        {
            var gameResult = await gameRepository.GetByIdAsync(gameId);
            if (gameResult.IsFailure)
            {
                return Result<OrderDto>.Failure(gameResult.Error!);
            }

            var activePromotionsResult = await promotionRepository.GetActivePromotionsByGameIdAsync(gameId);
            if (activePromotionsResult.IsFailure)
            {
                return Result<OrderDto>.Failure(activePromotionsResult.Error!);
            }

            var bestPromotion = activePromotionsResult.Value
                .OrderByDescending(promotion => promotion.DiscountPercentage)
                .FirstOrDefault();
            decimal priceAtPurchase = gameResult.Value.Price;

            if (bestPromotion != null)
            {
                priceAtPurchase -= bestPromotion.CalculateDiscountAmount(gameResult.Value.Price);
            }

            var addItemResult = order.AddItem(gameId, priceAtPurchase);
            if (addItemResult.IsFailure)
            {
                return Result<OrderDto>.Failure(addItemResult.Error!);
            }
        }

        var addResult = await orderRepository.AddAsync(order);
        if (addResult.IsFailure)
        {
            return Result<OrderDto>.Failure(addResult.Error!);
        }

        var commitResult = await unitOfWork.CommitAsync();
        return commitResult.IsFailure
            ? Result<OrderDto>.Failure(commitResult.Error!)
            : Result<OrderDto>.Success(MapToDto(order));
    }

    public async Task<Result<OrderDto>> GetOrderByIdAsync(Guid orderId, Guid currentUserId, bool isAdmin)
    {
        var orderResult = await orderRepository.GetByIdAsync(orderId);
        if (orderResult.IsFailure)
        {
            return Result<OrderDto>.Failure(orderResult.Error!);
        }

        return CanAccessOrder(orderResult.Value, currentUserId, isAdmin)
            ? Result<OrderDto>.Success(MapToDto(orderResult.Value))
            : Result<OrderDto>.Failure(Errors.Orders.AccessDenied);
    }

    public async Task<Result<IEnumerable<OrderDto>>> GetUserOrdersAsync(Guid userId)
    {
        var ordersResult = await orderRepository.GetByUserIdAsync(userId);
        return ordersResult.IsFailure
            ? Result<IEnumerable<OrderDto>>.Failure(ordersResult.Error!)
            : Result<IEnumerable<OrderDto>>.Success(ordersResult.Value.Select(MapToDto));
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto(
            order.Id,
            order.UserId,
            order.CreatedAt,
            order.TotalAmount,
            order.Status.ToString(),
            order.Items.Select(orderItem => new OrderItemDto(
                orderItem.GameId,
                orderItem.Game?.Title ?? "Jogo Desconhecido",
                orderItem.PriceAtPurchase
            )).ToList()
        );
    }

    private static bool CanAccessOrder(Order order, Guid currentUserId, bool isAdmin)
    {
        return isAdmin || order.UserId == currentUserId;
    }
}
