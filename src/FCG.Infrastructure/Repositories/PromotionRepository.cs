using FCG.Domain.Common;
using FCG.Domain.Entities;
using FCG.Domain.Interfaces;
using FCG.Infrastructure.Data;
using FCG.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infrastructure.Repositories;

public class PromotionRepository(AppDbContext context)
    : Repository<Promotion>(context, Errors.Promotions.NotFound), IPromotionRepository
{
    protected override IQueryable<Promotion> Query => Context.Promotions.Include(promotion => promotion.Game);

    public async Task<Result<PagedResult<Promotion>>> GetAllAsync(PaginationParameters pagination)
    {
        var query = Context.Promotions
            .Include(promotion => promotion.Game)
            .OrderBy(promotion => promotion.Name);

        return Result<PagedResult<Promotion>>.Success(await query.ToPagedResultAsync(pagination));
    }

    public async Task<Result<PagedResult<Promotion>>> GetAllActiveAsync(PaginationParameters pagination)
    {
        var now = DateTime.UtcNow;
        var query = Context.Promotions
            .Include(promotion => promotion.Game)
            .Where(promotion => promotion.IsActive && promotion.StartDate <= now && promotion.EndDate >= now)
            .OrderBy(promotion => promotion.Name);

        return Result<PagedResult<Promotion>>.Success(await query.ToPagedResultAsync(pagination));
    }

    public async Task<Result<IEnumerable<Promotion>>> GetActivePromotionsByGameIdAsync(Guid gameId)
    {
        var now = DateTime.UtcNow;
        var promotions = await Context.Promotions
            .Where(promotion => promotion.GameId == gameId && promotion.IsActive && promotion.StartDate <= now && promotion.EndDate >= now)
            .ToListAsync();

        return Result<IEnumerable<Promotion>>.Success(promotions);
    }
}
