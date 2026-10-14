using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Features.Orders.Dtos;
public sealed record OrderListItemDto(long Id, long CustomerId, OrderStatus Status, decimal TotalAmount, DateTimeOffset CreatedAt);
