using MediatR;
using OrderManagement.Application.Common.Pagination;
using OrderManagement.Application.Common.Results;
using OrderManagement.Application.Features.Orders.Models;

namespace OrderManagement.Application.Features.Orders.Queries;

public sealed record GetOrderByIdQuery(long OrderId) : IRequest<Result<OrderDetailsDto>>;
public sealed record GetOrdersQuery(OrderFilter Filter, int PageNumber = 1, int PageSize = 20)
    : IRequest<Result<OffsetPage<OrderListItemDto>>>;
public sealed record GetOrdersAfterQuery(OrderFilter Filter, long? AfterId = null, int PageSize = 20)
    : IRequest<Result<CursorPage<OrderListItemDto>>>;
