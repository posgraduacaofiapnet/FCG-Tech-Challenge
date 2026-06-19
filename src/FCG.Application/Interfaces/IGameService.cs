using FCG.Application.DTOs;
using FCG.Domain.Common;

namespace FCG.Application.Interfaces;

public interface IGameService
{
    Task<Result<PagedResult<GameDto>>> GetAllAsync(int page);
    Task<Result<GameDto>> GetByIdAsync(Guid id);
    Task<Result<GameDto>> CreateAsync(CreateGameDto dto);
    Task<Result> AddToLibraryAsync(Guid userId, Guid gameId);
    Task<Result<IEnumerable<GameDto>>> GetLibraryAsync(Guid userId);
}
