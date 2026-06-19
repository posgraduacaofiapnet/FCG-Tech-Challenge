using FCG.Domain.Common;
using FCG.Domain.Entities;

namespace FCG.Domain.Interfaces;

public interface IPromotionRepository : IRepository<Promotion>
{
    Task<Result<PagedResult<Promotion>>> GetAllAsync(PaginationParameters pagination);
    Task<Result<PagedResult<Promotion>>> GetAllActiveAsync(PaginationParameters pagination);
    Task<Result<IEnumerable<Promotion>>> GetActivePromotionsByGameIdAsync(Guid gameId);
}
