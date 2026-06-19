using FCG.Domain.Common;
using FCG.Domain.Entities;

namespace FCG.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<Result<User>> GetByEmailAsync(string email);
    Task<Result<PagedResult<User>>> GetAllAsync(PaginationParameters pagination);
    Task<Result<User>> GetByIdIncludingInactiveAsync(Guid id);
    Task<Result<PagedResult<User>>> GetAllIncludingInactiveAsync(PaginationParameters pagination);
    Task<Result<User>> GetByEmailIncludingInactiveAsync(string email);
}
