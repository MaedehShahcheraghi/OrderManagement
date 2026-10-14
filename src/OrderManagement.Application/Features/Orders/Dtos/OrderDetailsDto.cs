using OrderManagement.Application.Features.Orders.Dtos;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Features.Orders.Models;

public sealed record OrderDetailsDto(
    long Id, long CustomerId, OrderStatus Status, decimal TotalAmount,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt,
    int Revision, byte[] RowVersion,
    IReadOnlyList<OrderItemDto> Items,
    IReadOnlyList<OrderStatusHistoryDto> StatusHistory);
