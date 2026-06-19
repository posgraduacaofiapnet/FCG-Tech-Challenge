using FCG.Domain.Common;
using FCG.Domain.Entities;

namespace FCG.Domain.Interfaces;

public interface IRepository<TEntity> where TEntity : Entity
{
    Task<Result<TEntity>> GetByIdAsync(Guid id);
    Task<Result> AddAsync(TEntity entity);
    Task<Result> UpdateAsync(TEntity entity);
    Task<Result> DeleteAsync(TEntity entity);
}
