using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Persistence.Contexts;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository(OrderDbContext dbContext)
    : ICustomerRepository
{
    public Task<bool> ExistsAsync(
        long id,
        CancellationToken cancellationToken = default) =>
        dbContext.Customers.AnyAsync(
            x => x.Id == id,
            cancellationToken);
}
