using FCG.Domain.Common;
using FCG.Domain.Entities;
using FCG.Domain.Interfaces;
using FCG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infrastructure.Repositories;

public abstract class Repository<TEntity>(AppDbContext context, Error notFoundError)
    : IRepository<TEntity> where TEntity : Entity
{
    protected AppDbContext Context { get; } = context;

    protected virtual IQueryable<TEntity> Query => Context.Set<TEntity>();

    public virtual async Task<Result<TEntity>> GetByIdAsync(Guid id)
    {
        var entity = await Query.FirstOrDefaultAsync(entity => entity.Id == id);

        return entity is null
            ? Result<TEntity>.Failure(notFoundError)
            : Result<TEntity>.Success(entity);
    }

    public virtual async Task<Result> AddAsync(TEntity entity)
    {
        await Context.Set<TEntity>().AddAsync(entity);
        return Result.Success();
    }

    public virtual Task<Result> UpdateAsync(TEntity entity)
    {
        var entry = Context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            Context.Set<TEntity>().Attach(entity);
            Context.Entry(entity).State = EntityState.Modified;
        }

        return Task.FromResult(Result.Success());
    }

    public virtual Task<Result> DeleteAsync(TEntity entity)
    {
        Context.Set<TEntity>().Remove(entity);
        return Task.FromResult(Result.Success());
    }
}
