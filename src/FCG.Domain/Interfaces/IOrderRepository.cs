using FCG.Domain.Common;
using FCG.Domain.Entities;

namespace FCG.Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Result<IEnumerable<Order>>> GetByUserIdAsync(Guid userId);
}
