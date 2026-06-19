using FCG.Domain.Common;
using FCG.Domain.Entities;
using FCG.Domain.Interfaces;
using FCG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infrastructure.Repositories;

public class OrderRepository(AppDbContext context)
    : Repository<Order>(context, Errors.Orders.NotFound), IOrderRepository
{
    protected override IQueryable<Order> Query => Context.Orders
        .Include(order => order.Items)
        .ThenInclude(orderItem => orderItem.Game)
        .Include(order => order.User);

    public async Task<Result<IEnumerable<Order>>> GetByUserIdAsync(Guid userId)
    {
        var orders = await Context.Orders
            .Include(order => order.Items)
                .ThenInclude(orderItem => orderItem.Game)
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync();

        return Result<IEnumerable<Order>>.Success(orders);
    }
}
