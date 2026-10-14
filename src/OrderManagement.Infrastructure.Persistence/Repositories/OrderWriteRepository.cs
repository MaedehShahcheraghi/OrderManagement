using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities.Orders;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Persistence.Contexts;
namespace OrderManagement.Infrastructure.Persistence.Repositories;
public sealed class OrderWriteRepository(OrderDbContext db) : IOrderWriteRepository
{
    // Tracked entity plus RowVersion; this name does not imply a SQL locking hint.
    public Task<Order?> GetForUpdateAsync(long id, CancellationToken cancellationToken = default) =>
        db.Orders.Include(x => x.Items).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    public void Add(Order order) => db.Orders.Add(order);
    public void AddRange(IEnumerable<Order> orders) => db.Orders.AddRange(orders);
}

