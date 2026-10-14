using MediatR;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Common.Errors;
using OrderManagement.Application.Common.Results;
using OrderManagement.Application.Common.Security;
using OrderManagement.Application.Features.Orders.Queries;
using OrderManagement.Application.Features.Orders.Models;
using OrderManagement.Application.Common.Pagination;

namespace OrderManagement.Application.Features.Orders;

public sealed class GetOrderByIdHandler(IOrderReadRepository orders, ICurrentUser user)
    : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailsDto>>
{
    public async Task<Result<OrderDetailsDto>> Handle(GetOrderByIdQuery request, CancellationToken token)
    {
        if (!user.IsAuthenticated) return Result<OrderDetailsDto>.Failure(CommonErrors.Unauthenticated);
        if (request.OrderId <= 0) return Result<OrderDetailsDto>.Failure(OrderErrors.InvalidId);
        if (!user.IsInRole(RoleNames.Admin) && user.CustomerId is null)
            return Result<OrderDetailsDto>.Failure(CommonErrors.Forbidden);
        var order = await orders.GetByIdAsync(request.OrderId, user.IsInRole(RoleNames.Admin) ? null : user.CustomerId, token);
        return order is null
            ? Result<OrderDetailsDto>.Failure(OrderErrors.NotFound(request.OrderId))
            : Result<OrderDetailsDto>.Success(order);
    }
}

public sealed class GetOrdersHandler(IOrderReadRepository orders, ICurrentUser user)
    : IRequestHandler<GetOrdersQuery, Result<OffsetPage<OrderListItemDto>>>
{
    public async Task<Result<OffsetPage<OrderListItemDto>>> Handle(GetOrdersQuery request, CancellationToken token)
    {
        if (!user.IsAuthenticated) return Result<OffsetPage<OrderListItemDto>>.Failure(CommonErrors.Unauthenticated);
        if (!user.IsInRole(RoleNames.Admin) && user.CustomerId is null)
            return Result<OffsetPage<OrderListItemDto>>.Failure(CommonErrors.Forbidden);
        if (request.PageNumber < 1 || request.PageSize is < 1 or > 100)
            return Result<OffsetPage<OrderListItemDto>>.Failure(new("orders.invalid_page", "Invalid pagination.", ErrorType.Validation));
        var filter = user.IsInRole(RoleNames.Admin) ? request.Filter : request.Filter with { CustomerId = user.CustomerId };
        if (filter.From.HasValue && filter.To.HasValue && filter.From >= filter.To)
            return Result<OffsetPage<OrderListItemDto>>.Failure(new("orders.invalid_dates", "From must precede To.", ErrorType.Validation));
        try
        {
            return Result<OffsetPage<OrderListItemDto>>.Success(
                await orders.GetPageAsync(filter, request.PageNumber, request.PageSize, token));
        }
        catch (ArgumentOutOfRangeException)
        {
            return Result<OffsetPage<OrderListItemDto>>.Failure(
                new("orders.invalid_page", "Page exceeds supported offset; use cursor pagination.", ErrorType.Validation));
        }
    }
}

public sealed class GetOrdersAfterHandler(IOrderReadRepository orders, ICurrentUser user)
    : IRequestHandler<GetOrdersAfterQuery, Result<CursorPage<OrderListItemDto>>>
{
    public async Task<Result<CursorPage<OrderListItemDto>>> Handle(GetOrdersAfterQuery request, CancellationToken token)
    {
        if (!user.IsAuthenticated) return Result<CursorPage<OrderListItemDto>>.Failure(CommonErrors.Unauthenticated);
        if (!user.IsInRole(RoleNames.Admin) && user.CustomerId is null)
            return Result<CursorPage<OrderListItemDto>>.Failure(CommonErrors.Forbidden);
        if (request.PageSize is < 1 or > 100 || request.AfterId is <= 0)
            return Result<CursorPage<OrderListItemDto>>.Failure(new("orders.invalid_cursor", "Invalid cursor or page size.", ErrorType.Validation));
        var filter = user.IsInRole(RoleNames.Admin) ? request.Filter : request.Filter with { CustomerId = user.CustomerId };
        if (filter.From.HasValue && filter.To.HasValue && filter.From >= filter.To)
            return Result<CursorPage<OrderListItemDto>>.Failure(new("orders.invalid_dates", "From must precede To.", ErrorType.Validation));
        return Result<CursorPage<OrderListItemDto>>.Success(
            await orders.GetAfterAsync(filter, request.AfterId, request.PageSize, token));
    }
}
