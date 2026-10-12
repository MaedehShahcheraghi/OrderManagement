using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities.Orders;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Persistence.Contexts;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

public sealed class OrderWriteRepository(OrderDbContext dbContext)
    : IOrderWriteRepository
{
    public Task<Order?> GetForUpdateAsync(
        long id,
        CancellationToken cancellationToken = default) =>
        dbContext.Orders
            .Include(x => x.Items)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

    public void Add(Order order) =>
        dbContext.Orders.Add(order);

    public void AddRange(IEnumerable<Order> orders) =>
        dbContext.Orders.AddRange(orders);
}
