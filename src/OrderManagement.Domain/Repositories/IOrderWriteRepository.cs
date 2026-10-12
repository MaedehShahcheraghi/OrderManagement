using OrderManagement.Domain.Entities.Orders;

namespace OrderManagement.Domain.Repositories;

public interface IOrderWriteRepository
{
    Task<Order?> GetForUpdateAsync(
        long id,
        CancellationToken cancellationToken = default);

    void Add(Order order);

    void AddRange(IEnumerable<Order> orders);
}
