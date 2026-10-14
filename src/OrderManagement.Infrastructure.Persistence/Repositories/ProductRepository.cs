using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities.Products;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Persistence.Contexts;
namespace OrderManagement.Infrastructure.Persistence.Repositories;
public sealed class ProductRepository(OrderDbContext db) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetByIdsAsync(
        IReadOnlyCollection<long> ids, CancellationToken cancellationToken = default)
    {
        var result = new List<Product>();
        foreach (var chunk in ids.Distinct().Chunk(1000))
            result.AddRange(await db.Products.AsNoTracking().Where(x => chunk.Contains(x.Id))
                .ToListAsync(cancellationToken));
        return result;
    }
}

