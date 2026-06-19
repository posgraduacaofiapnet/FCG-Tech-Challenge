using FCG.Domain.Common;
using FCG.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FCG.Infrastructure.Data;

public sealed class UnitOfWork(AppDbContext context, ILogger<UnitOfWork> logger) : IUnitOfWork
{
    public async Task<Result> CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                logger.LogError(
                    ex,
                    "Erro de concorrencia ao salvar {EntityType}. State: {State}. CurrentValues: {@CurrentValues}",
                    entry.Metadata.ClrType.Name,
                    entry.State,
                    entry.CurrentValues.ToObject());
            }

            return Result.Failure(Errors.UnitOfWork.CommitFailed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao salvar alteracoes no banco de dados.");
            return Result.Failure(Errors.UnitOfWork.CommitFailed);
        }
    }
}
