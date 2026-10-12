using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Orders.Queries;

public sealed record OrderListItemDto(
    long Id,
    long CustomerId,
    OrderStatus Status,
    decimal TotalAmount,
    DateTimeOffset CreatedAt);
