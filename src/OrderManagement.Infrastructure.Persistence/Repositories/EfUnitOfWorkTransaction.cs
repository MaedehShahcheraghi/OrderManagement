using Microsoft.EntityFrameworkCore.Storage;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

internal sealed class EfUnitOfWorkTransaction(
    IDbContextTransaction transaction,
    Action onRollback)
    : IUnitOfWorkTransaction
{
    private bool _completed;

    public async Task CommitAsync(
        CancellationToken cancellationToken = default)
    {
        if (_completed)
        {
            throw new InvalidOperationException(
                "The transaction has already completed.");
        }

        await transaction.CommitAsync(cancellationToken);

        _completed = true;
    }

    public async Task RollbackAsync(
        CancellationToken cancellationToken = default)
    {
        if (_completed)
            return;

        onRollback();

        await transaction.RollbackAsync(cancellationToken);

        _completed = true;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (!_completed)
            {
                await RollbackAsync(CancellationToken.None);
            }
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }
}
