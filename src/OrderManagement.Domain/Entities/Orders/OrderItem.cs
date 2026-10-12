using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Entities.Orders;

public sealed class OrderItem : Entity
{
    private OrderItem() { }

    public long OrderId { get; private set; }
    public long ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal => UnitPrice * Quantity;

    internal static OrderItem Create(OrderItemData data)
    {
        DomainGuard.PositiveId(data.ProductId, nameof(ProductId));
        DomainGuard.PositiveQuantity(data.Quantity);
        DomainGuard.PositiveMoney(data.UnitPrice, nameof(UnitPrice));
        DomainGuard.PositiveMoney(data.UnitPrice * data.Quantity, nameof(LineTotal));

        return new OrderItem
        {
            ProductId = data.ProductId,
            Quantity = data.Quantity,
            UnitPrice = data.UnitPrice
        };
    }
}
