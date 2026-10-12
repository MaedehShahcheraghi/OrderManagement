namespace OrderManagement.Domain.Repositories;


public interface IInventoryRepository
{
    Task<bool> TryDecreaseAsync(
        long productId,
        int quantity,
        CancellationToken cancellationToken = default);
}
