using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Persistence.Contexts;
namespace OrderManagement.Infrastructure.Persistence.Repositories;
public sealed class InventoryRepository(OrderDbContext db) : IInventoryRepository
{
    public async Task<bool> TryDecreaseAsync(long productId, int quantity,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(productId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Stock changes require an active transaction.");
        return await db.InventoryStocks
            .Where(x => x.ProductId == productId && x.AvailableQuantity >= quantity)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(x => x.AvailableQuantity,
                    x => x.AvailableQuantity - quantity), cancellationToken) == 1;
    }
}

