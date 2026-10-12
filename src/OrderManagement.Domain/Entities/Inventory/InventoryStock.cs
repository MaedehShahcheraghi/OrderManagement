using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Entities.Inventory;

public sealed class InventoryStock : Entity
{
    private InventoryStock() { }

    public long ProductId { get; private set; }
    public int AvailableQuantity { get; private set; }

    public static InventoryStock Create(long productId, int availableQuantity)
    {
        DomainGuard.PositiveId(productId, nameof(ProductId));

        if (availableQuantity < 0)
        {
            throw new DomainException("inventory.negative_stock", "Available stock cannot be negative.");
        }

        return new InventoryStock { ProductId = productId, AvailableQuantity = availableQuantity };
    }

    public void Decrease(int quantity)
    {
        DomainGuard.PositiveQuantity(quantity);

        if (AvailableQuantity < quantity)
        {
            throw new DomainException("inventory.insufficient_stock",
                $"Product {ProductId} does not have enough available stock.");
        }

        AvailableQuantity -= quantity;
    }

    public void Increase(int quantity)
    {
        DomainGuard.PositiveQuantity(quantity);

        if (quantity > int.MaxValue - AvailableQuantity)
        {
            throw new DomainException("inventory.stock_overflow", "Available stock exceeds its supported range.");
        }

        AvailableQuantity += quantity;
    }
}
