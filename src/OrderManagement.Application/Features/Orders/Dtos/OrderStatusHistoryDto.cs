using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Features.Orders.Dtos;
public sealed record OrderStatusHistoryDto(OrderStatus? FromStatus, OrderStatus ToStatus, DateTimeOffset ChangedAt, string ChangedBy, int OrderRevision);
