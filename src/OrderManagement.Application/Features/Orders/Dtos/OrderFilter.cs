using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Features.Orders.Dtos;

public sealed record OrderFilter(
    long? CustomerId = null,
    OrderStatus? Status = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null);
