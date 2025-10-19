namespace HouseLedger.Services.Finance.Application.Contracts.Common;

/// <summary>
/// Base class for paged requests.
/// </summary>
public class PagedRequest
{
    private const int MaxPageSize = 100;
    private int _pageSize = 50;

    /// <summary>
    /// Page number (1-based).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page (max 100).
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    /// <summary>
    /// Number of items to skip.
    /// </summary>
    public int Skip => (Page - 1) * PageSize;
}
