using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Persistence.Contexts;
namespace OrderManagement.Infrastructure.Persistence.Repositories;
public sealed class CustomerRepository(OrderDbContext db) : ICustomerRepository
{
    public Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default) =>
        db.Customers.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken);
    public async Task<IReadOnlySet<long>> GetExistingIdsAsync(
        IReadOnlyCollection<long> ids, CancellationToken cancellationToken = default)
    {
        var existing = new HashSet<long>();
        foreach (var chunk in ids.Distinct().Chunk(1000))
        {
            var found = await db.Customers.AsNoTracking().Where(x => chunk.Contains(x.Id))
                .Select(x => x.Id).ToListAsync(cancellationToken);
            existing.UnionWith(found);
        }
        return existing;
    }
}

