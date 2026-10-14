using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Common.Pagination;
using OrderManagement.Application.Features.Orders.Dtos;
using OrderManagement.Application.Features.Orders.Models;
using OrderManagement.Domain.Entities.Orders;
using OrderManagement.Infrastructure.Persistence.Contexts;
namespace OrderManagement.Infrastructure.Persistence.Repositories;
public sealed class OrderReadRepository(OrderDbContext db) : IOrderReadRepository
{
    private static readonly Expression<Func<Order, OrderListItemDto>> Projection =
        order => new OrderListItemDto(order.Id, order.CustomerId, order.Status,
            order.TotalAmount, order.CreatedAt);

    public Task<OrderDetailsDto?> GetByIdAsync(long id, long? customerScope,
        CancellationToken cancellationToken = default)
    {
        var query = db.Orders.AsNoTracking().Where(x => x.Id == id);
        if (customerScope.HasValue)
            query = query.Where(x => x.CustomerId == customerScope.Value);
        return query.AsSplitQuery().Select(order => new OrderDetailsDto(
            order.Id, order.CustomerId, order.Status, order.TotalAmount,
            order.CreatedAt, order.UpdatedAt, order.Revision,
            EF.Property<byte[]>(order, "RowVersion"),
            order.Items.OrderBy(x => x.ProductId)
                .Select(x => new OrderItemDto(x.ProductId, x.Quantity, x.UnitPrice)).ToList(),
            order.StatusHistory.OrderBy(x => x.OrderRevision)
                .Select(x => new OrderStatusHistoryDto(x.FromStatus, x.ToStatus,
                    x.ChangedAt, x.ChangedBy, x.OrderRevision)).ToList()
        )).SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<OffsetPage<OrderListItemDto>> GetPageAsync(OrderFilter filter,
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        ValidatePageSize(pageSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageNumber);
        long offset = ((long)pageNumber - 1) * pageSize;
        if (offset > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Use cursor pagination.");
        var query = ApplyFilter(filter);
        long total = await query.LongCountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.Id).Skip((int)offset)
            .Take(pageSize).Select(Projection).ToListAsync(cancellationToken);
        return new(items, pageNumber, pageSize, total);
    }

    public async Task<CursorPage<OrderListItemDto>> GetAfterAsync(OrderFilter filter,
        long? afterId, int pageSize, CancellationToken cancellationToken = default)
    {
        ValidatePageSize(pageSize);
        if (afterId.HasValue) ArgumentOutOfRangeException.ThrowIfNegativeOrZero(afterId.Value);
        var query = ApplyFilter(filter);
        if (afterId.HasValue) query = query.Where(x => x.Id < afterId.Value);
        var items = await query.OrderByDescending(x => x.Id).Take(pageSize + 1)
            .Select(Projection).ToListAsync(cancellationToken);
        bool hasNext = items.Count > pageSize;
        if (hasNext) items.RemoveAt(items.Count - 1);
        return new(items, hasNext, hasNext ? items[^1].Id : null);
    }

    private IQueryable<Order> ApplyFilter(OrderFilter filter)
    {
        var query = db.Orders.AsNoTracking();
        if (filter.CustomerId.HasValue)
            query = query.Where(x => x.CustomerId == filter.CustomerId.Value);
        if (filter.Status.HasValue)
            query = query.Where(x => x.Status == filter.Status.Value);
        DateTimeOffset? from = filter.From?.ToUniversalTime();
        DateTimeOffset? to = filter.To?.ToUniversalTime();
        if (from.HasValue) query = query.Where(x => x.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(x => x.CreatedAt < to.Value);
        return query;
    }

    private static void ValidatePageSize(int size)
    {
        if (size is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(size));
    }
}

