using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities.Products;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Persistence.Contexts;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(OrderDbContext dbContext)
    : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetByIdsAsync(
        IReadOnlyCollection<long> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return [];

        long[] productIds = [.. ids.Distinct()];

        return await dbContext.Products
            .AsNoTracking()
            .Where(x => productIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }
}
