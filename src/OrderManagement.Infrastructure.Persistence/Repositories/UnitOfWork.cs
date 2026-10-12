using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Common.Exceptions;
using OrderManagement.Domain.Entities.Orders;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Persistence.Contexts;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

public sealed class UnitOfWork(OrderDbContext dbContext)
    : IUnitOfWork
{
    private bool _aborted;

    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        EnsureUsable();

        try
        {
            return await dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new PersistenceConcurrencyException(exception);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqlException
            {
                Number: 2601 or 2627
            }
            && exception.Entries.Any(
                x => x.Entity is OrderStatusHistory))
        {
            throw new PersistenceConcurrencyException(exception);
        }
    }

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        EnsureUsable();

        if (dbContext.Database.CurrentTransaction is not null)
        {
            throw new InvalidOperationException(
                "Nested transactions are not supported.");
        }

        var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        return new EfUnitOfWorkTransaction(
            transaction,
            () => _aborted = true);
    }

    private void EnsureUsable()
    {
        if (_aborted)
        {
            throw new InvalidOperationException(
                "This unit of work was rolled back. Start a new scope.");
        }
    }
}

