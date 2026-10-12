using OrderManagement.Application.Common.Pagination;
using OrderManagement.Application.Features.Orders.Dtos;
using OrderManagement.Application.Orders.Queries;

namespace OrderManagement.Application.Abstractions;

public interface IOrderReadRepository
{
    Task<OrderDetailsDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<OffsetPage<OrderListItemDto>> GetPageAsync(
        OrderFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<CursorPage<OrderListItemDto>> GetAfterAsync(
        OrderFilter filter,
        long? afterId,
        int pageSize,
        CancellationToken cancellationToken = default);
}
