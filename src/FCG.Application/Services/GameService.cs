using FCG.Application.DTOs;
using FCG.Application.Interfaces;
using FCG.Application.Settings;
using FCG.Domain.Common;
using FCG.Domain.Entities;
using FCG.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace FCG.Application.Services;

public sealed class GameService(
    IGameRepository gameRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IOptions<PaginationSettings> paginationSettings) : IGameService
{
    private readonly int _pageSize = paginationSettings.Value.PageSize;

    public async Task<Result<GameDto>> CreateAsync(CreateGameDto dto)
    {
        var game = new Game(dto.Title, dto.Description, dto.Price);
        var addResult = await gameRepository.AddAsync(game);

        if (addResult.IsFailure)
        {
            return Result<GameDto>.Failure(addResult.Error!);
        }

        var commitResult = await unitOfWork.CommitAsync();
        return commitResult.IsFailure
            ? Result<GameDto>.Failure(commitResult.Error!)
            : Result<GameDto>.Success(MapToGameDto(game));
    }

    public async Task<Result<PagedResult<GameDto>>> GetAllAsync(int page)
    {
        if (page < 1)
        {
            return Result<PagedResult<GameDto>>.Failure(Errors.Pagination.InvalidPage);
        }

        var gamesResult = await gameRepository.GetAllAsync(new PaginationParameters(page, _pageSize));
        if (gamesResult.IsFailure)
        {
            return Result<PagedResult<GameDto>>.Failure(gamesResult.Error!);
        }

        var games = gamesResult.Value;
        return Result<PagedResult<GameDto>>.Success(new PagedResult<GameDto>(
            games.Items.Select(MapToGameDto).ToArray(),
            games.Page,
            games.PageSize,
            games.TotalCount));
    }

    public async Task<Result<GameDto>> GetByIdAsync(Guid id)
    {
        var gameResult = await gameRepository.GetByIdAsync(id);
        return gameResult.IsFailure
            ? Result<GameDto>.Failure(gameResult.Error!)
            : Result<GameDto>.Success(MapToGameDto(gameResult.Value));
    }

    public async Task<Result> AddToLibraryAsync(Guid userId, Guid gameId)
    {
        var userResult = await userRepository.GetByIdAsync(userId);
        if (userResult.IsFailure)
        {
            return Result.Failure(userResult.Error!);
        }

        var gameResult = await gameRepository.GetByIdAsync(gameId);
        if (gameResult.IsFailure)
        {
            return Result.Failure(gameResult.Error!);
        }

        userResult.Value.AddGameToLibrary(gameResult.Value);
        var updateResult = await userRepository.UpdateAsync(userResult.Value);
        return updateResult.IsFailure ? updateResult : await unitOfWork.CommitAsync();
    }

    public async Task<Result<IEnumerable<GameDto>>> GetLibraryAsync(Guid userId)
    {
        var userResult = await userRepository.GetByIdAsync(userId);
        return userResult.IsFailure
            ? Result<IEnumerable<GameDto>>.Failure(userResult.Error!)
            : Result<IEnumerable<GameDto>>.Success(userResult.Value.LibraryItems.Select(libraryItem => MapToGameDto(libraryItem.Game)));
    }

    private static GameDto MapToGameDto(Game game)
    {
        var now = DateTime.UtcNow;
        var activePromotion = game.Promotions
            .Where(promotion => promotion.IsActive && promotion.StartDate <= now && promotion.EndDate >= now)
            .OrderByDescending(promotion => promotion.DiscountPercentage)
            .FirstOrDefault();

        if (activePromotion != null)
        {
            var discountedPrice = game.Price - (game.Price * activePromotion.DiscountPercentage / 100);
            return new GameDto(
                game.Id,
                game.Title,
                game.Description,
                discountedPrice,
                game.Price,
                activePromotion.DiscountPercentage,
                activePromotion.Name,
                true
            );
        }

        return new GameDto(game.Id, game.Title, game.Description, game.Price, game.Price);
    }
}
