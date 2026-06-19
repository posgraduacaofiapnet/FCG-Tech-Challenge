using FCG.Domain.Common;
using FCG.Domain.Entities;
using FCG.Domain.Interfaces;
using FCG.Infrastructure.Data;
using FCG.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infrastructure.Repositories;

public class GameRepository(AppDbContext context)
    : Repository<Game>(context, Errors.Games.NotFound), IGameRepository
{
    protected override IQueryable<Game> Query => Context.Games.Include(game => game.Promotions);

    public async Task<Result<PagedResult<Game>>> GetAllAsync(PaginationParameters pagination)
    {
        var query = Context.Games
            .Include(game => game.Promotions)
            .Where(game => game.IsActive)
            .OrderBy(game => game.Title);

        return Result<PagedResult<Game>>.Success(await query.ToPagedResultAsync(pagination));
    }
}
