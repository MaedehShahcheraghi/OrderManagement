namespace OrderManagement.Application.Common.Pagination;

public sealed record OffsetPage<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    long TotalCount)
{
    public long TotalPages =>
        (long)Math.Ceiling((decimal)TotalCount / PageSize);
}
