using OrderManagement.Domain.Entities.Orders;
namespace OrderManagement.Domain.Repositories;
public interface IBulkOrderWriteRepository
{
    void AddRange(IReadOnlyCollection<Order> orders);
}
