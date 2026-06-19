using FCG.Application.DTOs;
using FCG.Application.Interfaces;
using FCG.Application.Settings;
using FCG.Domain.Common;
using FCG.Domain.Entities;
using FCG.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace FCG.Application.Services;

public sealed class PromotionService(
    IPromotionRepository promotionRepository,
    IGameRepository gameRepository,
    IUnitOfWork unitOfWork,
    IOptions<PaginationSettings> paginationSettings) : IPromotionService
{
    private readonly int _pageSize = paginationSettings.Value.PageSize;

    public async Task<Result<PagedResult<PromotionDto>>> GetAllActiveAsync(int page)
    {
        if (page < 1)
        {
            return Result<PagedResult<PromotionDto>>.Failure(Errors.Pagination.InvalidPage);
        }

        var promotionsResult = await promotionRepository.GetAllActiveAsync(new PaginationParameters(page, _pageSize));
        if (promotionsResult.IsFailure)
        {
            return Result<PagedResult<PromotionDto>>.Failure(promotionsResult.Error!);
        }

        var promotions = promotionsResult.Value;
        return Result<PagedResult<PromotionDto>>.Success(new PagedResult<PromotionDto>(
            promotions.Items.Select(MapToDto).ToArray(),
            promotions.Page,
            promotions.PageSize,
            promotions.TotalCount));
    }

    public async Task<Result<PromotionDto>> CreateAsync(CreatePromotionDto dto)
    {
        var gameResult = await gameRepository.GetByIdAsync(dto.GameId);
        if (gameResult.IsFailure)
        {
            return Result<PromotionDto>.Failure(gameResult.Error!);
        }

        var promotion = new Promotion(
            dto.Name,
            dto.GameId,
            dto.DiscountPercentage,
            dto.StartDate,
            dto.EndDate
        );

        var addResult = await promotionRepository.AddAsync(promotion);
        if (addResult.IsFailure)
        {
            return Result<PromotionDto>.Failure(addResult.Error!);
        }

        var commitResult = await unitOfWork.CommitAsync();
        return commitResult.IsFailure
            ? Result<PromotionDto>.Failure(commitResult.Error!)
            : Result<PromotionDto>.Success(MapToDto(promotion, gameResult.Value.Title));
    }

    public async Task<Result> DeactivateAsync(Guid id)
    {
        var promotionResult = await promotionRepository.GetByIdAsync(id);
        if (promotionResult.IsFailure)
        {
            return Result.Failure(promotionResult.Error!);
        }

        promotionResult.Value.Deactivate();
        var updateResult = await promotionRepository.UpdateAsync(promotionResult.Value);
        return updateResult.IsFailure ? updateResult : await unitOfWork.CommitAsync();
    }

    private static PromotionDto MapToDto(Promotion promotion)
    {
        return MapToDto(promotion, promotion.Game.Title);
    }

    private static PromotionDto MapToDto(Promotion promotion, string gameTitle)
    {
        return new PromotionDto(
            promotion.Id,
            promotion.Name,
            promotion.GameId,
            gameTitle,
            promotion.DiscountPercentage,
            promotion.StartDate,
            promotion.EndDate,
            promotion.IsActive
        );
    }
}
