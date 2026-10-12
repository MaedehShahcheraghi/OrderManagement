using OrderManagement.Domain.Common;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Entities.Orders;

public sealed class OrderStatusHistory : Entity
{
    private OrderStatusHistory() { }

    public long OrderId { get; private set; }
    public OrderStatus? FromStatus { get; private set; }
    public OrderStatus ToStatus { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
    public string ChangedBy { get; private set; } = string.Empty;
    public int OrderRevision { get; private set; }

    internal static OrderStatusHistory Create(
        OrderStatus? fromStatus,
        OrderStatus toStatus,
        DateTimeOffset changedAt,
        string changedBy,
        int orderRevision)
    {
        return new OrderStatusHistory
        {
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedAt = changedAt,
            ChangedBy = changedBy,
            OrderRevision = orderRevision
        };
    }
}
