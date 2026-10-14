using OrderManagement.Domain.Entities.Orders;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Persistence.Contexts;
namespace OrderManagement.Infrastructure.Persistence.Repositories;
public sealed class BulkOrderWriteRepository(OrderDbContext db) : IBulkOrderWriteRepository
{
    public void AddRange(IReadOnlyCollection<Order> orders) => db.Orders.AddRange(orders);
}
