namespace OrderManagement.Domain.Repositories;


public interface ICustomerRepository
{
    Task<bool> ExistsAsync(
        long id,
        CancellationToken cancellationToken = default);
}
