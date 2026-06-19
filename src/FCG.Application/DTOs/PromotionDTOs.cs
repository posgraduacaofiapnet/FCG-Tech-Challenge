namespace FCG.Application.DTOs;

public record CreatePromotionDto(
    string Name,
    Guid GameId,
    decimal DiscountPercentage,
    DateTime StartDate,
    DateTime EndDate
);
public record PromotionDto(
    Guid Id,
    string Name,
    Guid GameId,
    string GameTitle,
    decimal DiscountPercentage,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive
);