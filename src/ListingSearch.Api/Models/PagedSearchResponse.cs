namespace ListingSearch.Api.Models;

/// <summary>
/// Standard paged response returned by the search endpoint.
/// </summary>
public sealed class PagedSearchResponse
{
    public IReadOnlyList<ListingSearchResult> Items { get; init; } = Array.Empty<ListingSearchResult>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalResults { get; init; }
    public int TotalPages { get; init; }
}
