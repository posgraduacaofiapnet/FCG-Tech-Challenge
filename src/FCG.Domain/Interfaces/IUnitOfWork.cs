using FCG.Domain.Common;

namespace FCG.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<Result> CommitAsync(CancellationToken cancellationToken = default);
}
