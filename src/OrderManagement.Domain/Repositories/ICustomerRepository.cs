namespace OrderManagement.Domain.Repositories;
public interface ICustomerRepository
{
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlySet<long>> GetExistingIdsAsync(
        IReadOnlyCollection<long> ids, CancellationToken cancellationToken = default);
}

