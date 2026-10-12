using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Common.Pagination;
using OrderManagement.Application.Features.Orders.Dtos;
using OrderManagement.Application.Orders.Queries;
using OrderManagement.Domain.Entities.Orders;
using OrderManagement.Infrastructure.Persistence.Contexts;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

public sealed class OrderReadRepository(OrderDbContext dbContext)
    : IOrderReadRepository
{
    private static readonly Expression<Func<Order, OrderListItemDto>>
        ListProjection =
            order => new OrderListItemDto(
                order.Id,
                order.CustomerId,
                order.Status,
                order.TotalAmount,
                order.CreatedAt);

    public Task<OrderDetailsDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default) =>
        dbContext.Orders
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(order => new OrderDetailsDto(
                order.Id,
                order.CustomerId,
                order.Status,
                order.TotalAmount,
                order.CreatedAt,
                order.Revision,
                EF.Property<byte[]>(order, "RowVersion"),
                order.Items
                    .OrderBy(item => item.ProductId)
                    .Select(item => new OrderItemDto(
                        item.ProductId,
                        item.Quantity,
                        item.UnitPrice))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<OffsetPage<OrderListItemDto>> GetPageAsync(
        OrderFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ValidatePageSize(pageSize);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            pageNumber);

        long offset = ((long)pageNumber - 1) * pageSize;

        if (offset > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                "Use keyset pagination for this range.");
        }

        IQueryable<Order> query = ApplyFilter(filter);

        long totalCount = await query.LongCountAsync(
            cancellationToken);

        List<OrderListItemDto> items = await query
            .OrderByDescending(x => x.Id)
            .Skip((int)offset)
            .Take(pageSize)
            .Select(ListProjection)
            .ToListAsync(cancellationToken);

        return new OffsetPage<OrderListItemDto>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }

    public async Task<CursorPage<OrderListItemDto>> GetAfterAsync(
        OrderFilter filter,
        long? afterId,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ValidatePageSize(pageSize);

        if (afterId.HasValue)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
                afterId.Value);
        }

        IQueryable<Order> query = ApplyFilter(filter);

        if (afterId.HasValue)
        {
            query = query.Where(x => x.Id < afterId.Value);
        }

        List<OrderListItemDto> items = await query
            .OrderByDescending(x => x.Id)
            .Take(pageSize + 1)
            .Select(ListProjection)
            .ToListAsync(cancellationToken);

        bool hasNextPage = items.Count > pageSize;

        if (hasNextPage)
        {
            items.RemoveAt(items.Count - 1);
        }

        long? nextAfterId = hasNextPage
            ? items[^1].Id
            : null;

        return new CursorPage<OrderListItemDto>(
            items,
            hasNextPage,
            nextAfterId);
    }

    private IQueryable<Order> ApplyFilter(OrderFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        if (filter.CustomerId.HasValue)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
                filter.CustomerId.Value);
        }

        if (filter.Status.HasValue &&
            !Enum.IsDefined(filter.Status.Value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(filter.Status));
        }

        DateTimeOffset? from =
            filter.From?.ToUniversalTime();

        DateTimeOffset? to =
            filter.To?.ToUniversalTime();

        if (from.HasValue &&
            to.HasValue &&
            from.Value >= to.Value)
        {
            throw new ArgumentException(
                "From must be earlier than To.");
        }

        IQueryable<Order> query =
            dbContext.Orders.AsNoTracking();

        if (filter.CustomerId.HasValue)
        {
            query = query.Where(x =>
                x.CustomerId == filter.CustomerId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(x =>
                x.Status == filter.Status.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(x =>
                x.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x =>
                x.CreatedAt < to.Value);
        }

        return query;
    }

    private static void ValidatePageSize(int pageSize)
    {
        if (pageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                "Page size must be between 1 and 100.");
        }
    }
}
