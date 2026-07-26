namespace SmartTaskManagement.Application.Common;

public class QueryParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;
    private int _page = 1;
    private string? _search;

    public string? Search
    {
        get => _search;
        // Handle empty spaces and sanitize input automatically
        set => _search = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public string? SortBy { get; set; }

    public bool SortDescending { get; set; } = false;

    public int Page
    {
        get => _page;
        // Ensure Page is never less than 1 (prevents DB Skip() errors)
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        // Ensure PageSize is between 1 and MaxPageSize
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value;
    }
}