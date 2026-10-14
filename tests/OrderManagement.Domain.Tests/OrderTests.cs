using OrderManagement.Domain.Common;
using OrderManagement.Domain.Entities.Inventory;
using OrderManagement.Domain.Entities.Orders;
using OrderManagement.Domain.Enums;
using Xunit;

namespace OrderManagement.Domain.Tests;

public class OrderTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);

    private static Order NewOrder() => Order.Create(1,
        [new OrderItemData(2, 3, 12.50m)], Now, "tester");

    [Fact]
    public void New_order_is_pending_and_uses_price_snapshots()
    {
        var order = NewOrder();
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(37.50m, order.TotalAmount);
        Assert.Equal(1, order.Revision);
        Assert.Single(order.StatusHistory);
    }

    [Fact]
    public void Duplicate_products_are_rejected()
    {
        Assert.Throws<DomainException>(() => Order.Create(1,
            [new OrderItemData(2, 1, 2m), new OrderItemData(2, 2, 2m)],
            Now, "tester"));
    }

    [Fact]
    public void Order_requires_at_least_one_item()
    {
        Assert.Throws<DomainException>(() => Order.Create(
            1, Array.Empty<OrderItemData>(), Now, "tester"));
    }

    [Fact]
    public void Status_changes_follow_defined_order()
    {
        var order = NewOrder();
        order.Confirm(Now.AddMinutes(1), "admin");
        order.Ship(Now.AddMinutes(2), "admin");
        order.Deliver(Now.AddMinutes(3), "admin");
        Assert.Equal(OrderStatus.Delivered, order.Status);
        Assert.Equal(4, order.Revision);
        Assert.Equal(4, order.StatusHistory.Count);
    }

    [Fact]
    public void Cannot_ship_pending_order()
    {
        var order = NewOrder();
        Assert.Throws<DomainException>(() => order.Ship(Now, "admin"));
    }

    [Fact]
    public void Repeated_confirmation_is_rejected()
    {
        var order = NewOrder();
        order.Confirm(Now, "admin");
        Assert.Throws<DomainException>(() => order.Confirm(Now, "admin"));
    }

    [Fact]
    public void Deleted_order_cannot_be_confirmed()
    {
        var order = NewOrder();
        order.Delete(Now.AddMinutes(1), "admin");
        Assert.True(order.IsDeleted);
        Assert.Throws<DomainException>(() => order.Confirm(Now.AddMinutes(2), "admin"));
    }

    [Fact]
    public void Stock_rejects_insufficient_quantity()
    {
        var stock = InventoryStock.Create(1, 5);
        Assert.Throws<DomainException>(() => stock.Decrease(6));
        Assert.Equal(5, stock.AvailableQuantity);
    }

    [Fact]
    public void Stock_decreases_with_valid_quantity()
    {
        var stock = InventoryStock.Create(1, 5);
        stock.Decrease(3);
        Assert.Equal(2, stock.AvailableQuantity);
    }
}
