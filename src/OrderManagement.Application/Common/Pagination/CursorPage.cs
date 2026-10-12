namespace OrderManagement.Application.Common.Pagination;


public sealed record CursorPage<T>(
    IReadOnlyList<T> Items,
    bool HasNextPage,
    long? NextAfterId);
