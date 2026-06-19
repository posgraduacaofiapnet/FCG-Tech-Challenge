using FCG.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infrastructure.Extensions;

public static class PaginationExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PaginationParameters pagination)
    {
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PagedResult<T>(items, pagination.Page, pagination.PageSize, totalCount);
    }
}
