using MediatR;
using OrderManagement.Application.Common.Results;
using OrderManagement.Application.Features.Orders.Models;

namespace OrderManagement.Application.Features.Orders.Commands;

public sealed record NewOrderItem(long ProductId, int Quantity);
public sealed record NewOrder(long CustomerId, IReadOnlyList<NewOrderItem> Items);
public sealed record CreateOrderCommand(long CustomerId, IReadOnlyList<NewOrderItem> Items)
    : IRequest<Result<long>>;
public sealed record BulkCreateOrdersCommand(IReadOnlyList<NewOrder> Orders)
    : IRequest<Result<IReadOnlyList<long>>>;
public sealed record ShipOrderCommand(long OrderId) : IRequest<Result>;
public sealed record DeliverOrderCommand(long OrderId) : IRequest<Result>;
public sealed record DeleteOrderCommand(long OrderId) : IRequest<Result>;
