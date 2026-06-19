namespace FCG.Application.DTOs;

public record CreateGameDto(string Title, string Description, decimal Price);
public record GameDto(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    decimal OriginalPrice,
    decimal? DiscountPercentage = null,
    string? PromotionName = null,
    bool HasActivePromotion = false
);
