namespace SmartTaskManagement.Application.Common;

public sealed class PagedResult<T>
{
    // IReadOnlyList ensures the data is materialized and prevents multiple DB enumerations
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    // ── Factory Methods

    public static PagedResult<T> Create(IEnumerable<T> items, int totalCount, int page, int pageSize) =>
        new()
        {
            // Safely materialize the collection if it hasn't been already
            Items = items as IReadOnlyList<T> ?? items.ToList().AsReadOnly(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

    // Very useful for early returns when TotalCount is known to be 0
    public static PagedResult<T> Empty(int page, int pageSize) =>
        new()
        {
            Items = Array.Empty<T>(),
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };
}