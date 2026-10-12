
using OrderManagement.Domain.Common;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Entities.Orders;

public sealed class Order : Entity
{
    public const int MaximumItems = 100;

    private readonly List<OrderItem> _items = [];
    private readonly List<OrderStatusHistory> _statusHistory = [];

    private Order()
    {
        Items = _items.AsReadOnly();
        StatusHistory = _statusHistory.AsReadOnly();
    }

    public long CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public int Revision { get; private set; } = 1;

    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    public IReadOnlyCollection<OrderItem> Items { get; }
    public IReadOnlyCollection<OrderStatusHistory> StatusHistory { get; }

    public static Order Create(
        long customerId,
        IReadOnlyCollection<OrderItemData> items,
        DateTimeOffset now,
        string createdBy)
    {
        DomainGuard.PositiveId(customerId, nameof(CustomerId));
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count is < 1 or > MaximumItems)
            throw new DomainException(
                "order.invalid_item_count",
                $"Order must contain 1 to {MaximumItems} items.");

        string actor = DomainGuard.RequiredText(
            createdBy, 200, nameof(createdBy));

        var timestamp = now.ToUniversalTime();

        var order = new Order
        {
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            CreatedAt = timestamp,
            UpdatedAt = timestamp
        };

        order._items.EnsureCapacity(items.Count);
        var productIds = new HashSet<long>(items.Count);

        foreach (var data in items)
        {
            if (data is null)
                throw new DomainException(
                    "order.invalid_item",
                    "Order item cannot be null.");

            var item = OrderItem.Create(data);

            if (!productIds.Add(item.ProductId))
                throw new DomainException(
                    "order.duplicate_product",
                    "Duplicate products are not allowed.");

            order._items.Add(item);
            order.TotalAmount += item.LineTotal;

            DomainGuard.PositiveMoney(
                order.TotalAmount, nameof(TotalAmount));
        }

        order._statusHistory.Add(
            OrderStatusHistory.Create(
                null,
                OrderStatus.Pending,
                timestamp,
                actor,
                order.Revision));

        return order;
    }

    public void Confirm(DateTimeOffset now, string changedBy) =>
        ChangeStatus(OrderStatus.Pending, OrderStatus.Confirmed, now, changedBy);

    public void Ship(DateTimeOffset now, string changedBy) =>
        ChangeStatus(OrderStatus.Confirmed, OrderStatus.Shipped, now, changedBy);

    public void Deliver(DateTimeOffset now, string changedBy) =>
        ChangeStatus(OrderStatus.Shipped, OrderStatus.Delivered, now, changedBy);

    public void Delete(DateTimeOffset now, string deletedBy)
    {
        if (IsDeleted)
            throw new DomainException(
                "order.already_deleted",
                "Order is already deleted.");

        string actor = DomainGuard.RequiredText(
            deletedBy, 200, nameof(deletedBy));

        var timestamp = ValidateTimestamp(now);
        int nextRevision = checked(Revision + 1);

        IsDeleted = true;
        DeletedAt = timestamp;
        DeletedBy = actor;
        UpdatedAt = timestamp;
        Revision = nextRevision;
    }

    private void ChangeStatus(
        OrderStatus expected,
        OrderStatus next,
        DateTimeOffset now,
        string changedBy)
    {
        if (IsDeleted)
            throw new DomainException(
                "order.already_deleted",
                "Deleted orders cannot be modified.");

        if (Status != expected)
            throw new DomainException(
                "order.invalid_status_transition",
                $"Cannot change status from {Status} to {next}.");

        string actor = DomainGuard.RequiredText(
            changedBy, 200, nameof(changedBy));

        var timestamp = ValidateTimestamp(now);
        int nextRevision = checked(Revision + 1);

        var history = OrderStatusHistory.Create(
            Status, next, timestamp, actor, nextRevision);

        _statusHistory.Add(history);

        Status = next;
        UpdatedAt = timestamp;
        Revision = nextRevision;
    }

    private DateTimeOffset ValidateTimestamp(DateTimeOffset now)
    {
        var timestamp = now.ToUniversalTime();

        if (timestamp < UpdatedAt)
            throw new DomainException(
                "order.invalid_timestamp",
                "Timestamp cannot be earlier than the previous update.");

        return timestamp;
    }
}
