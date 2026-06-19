using FCG.Domain.Common;
using FCG.Domain.Entities;

namespace FCG.Domain.Interfaces;

public interface IGameRepository : IRepository<Game>
{
    Task<Result<PagedResult<Game>>> GetAllAsync(PaginationParameters pagination);
}
