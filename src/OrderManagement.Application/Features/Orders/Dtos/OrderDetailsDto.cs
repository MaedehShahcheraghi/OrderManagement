using OrderManagement.Application.Orders.Queries;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Features.Orders.Dtos;

public sealed record OrderDetailsDto(
    long Id,
    long CustomerId,
    OrderStatus Status,
    decimal TotalAmount,
    DateTimeOffset CreatedAt,
    int Revision,
    byte[] RowVersion,
    IReadOnlyList<OrderItemDto> Items);
