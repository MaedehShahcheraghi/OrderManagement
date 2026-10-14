using OrderManagement.Domain.Entities.Products;
namespace OrderManagement.Domain.Repositories;
public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetByIdsAsync(
        IReadOnlyCollection<long> ids, CancellationToken cancellationToken = default);
}

