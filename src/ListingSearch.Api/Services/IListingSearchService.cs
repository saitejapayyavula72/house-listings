using ListingSearch.Api.Models;

namespace ListingSearch.Api.Services;

public interface IListingSearchService
{
    Task<PagedSearchResponse> SearchAsync(SearchRequest request, CancellationToken cancellationToken);
}
