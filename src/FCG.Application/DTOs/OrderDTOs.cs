namespace FCG.Application.DTOs;

public record CreateOrderDto(List<Guid> GameIds);

public record OrderDto(
    Guid Id,
    Guid UserId,
    DateTime CreatedAt,
    decimal TotalAmount,
    string Status,
    List<OrderItemDto> Items
);

public record OrderItemDto(
    Guid GameId,
    string GameTitle,
    decimal PriceAtPurchase
);
