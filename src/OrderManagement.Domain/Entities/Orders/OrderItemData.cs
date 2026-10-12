namespace OrderManagement.Domain.Entities.Orders;

public sealed record OrderItemData(long ProductId, int Quantity, decimal UnitPrice);
