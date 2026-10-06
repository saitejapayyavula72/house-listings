using ListingSearch.Api.Models;
using ListingSearch.Api.Repositories;

namespace ListingSearch.Api.Services;

/// <summary>
/// Applies search filters, calculates relevance, orders results deterministically,
/// and returns only the requested page.
/// </summary>
public sealed class ListingSearchService : IListingSearchService
{
    private readonly IListingRepository _repository;
    private readonly IListingScoringService _scoringService;
    private readonly ILogger<ListingSearchService> _logger;

    public ListingSearchService(
        IListingRepository repository,
        IListingScoringService scoringService,
        ILogger<ListingSearchService> logger)
    {
        _repository = repository;
        _scoringService = scoringService;
        _logger = logger;
    }

    public async Task<PagedSearchResponse> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
    {
        var listings = await _repository.GetAllAsync(cancellationToken);

        IEnumerable<Listing> filtered = listings;

        if (request.MinPrice.HasValue)
        {
            filtered = filtered.Where(x => x.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            filtered = filtered.Where(x => x.Price <= request.MaxPrice.Value);
        }

        if (request.MinBedrooms.HasValue)
        {
            filtered = filtered.Where(x => x.Bedrooms >= request.MinBedrooms.Value);
        }

        string city = request.City?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(city))
        {
            filtered = filtered.Where(x => string.Equals(x.City?.Trim(), city, StringComparison.OrdinalIgnoreCase));
        }

        string keyword = request.Keyword?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            // Requirement says the keyword is matched against description, so address/city/etc.
            // are deliberately not included here.
            filtered = filtered.Where(x =>
                !string.IsNullOrEmpty(x.Description) &&
                x.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        var rankedResults = filtered
            .Select(listing => new ListingSearchResult
            {
                Id = listing.Id,
                Source = listing.Source,
                Address = listing.Address,
                City = listing.City,
                State = listing.State,
                Zip = listing.Zip,
                Price = listing.Price,
                Bedrooms = listing.Bedrooms,
                Bathrooms = listing.Bathrooms,
                Sqft = listing.Sqft,
                Latitude = listing.Latitude,
                Longitude = listing.Longitude,
                ListedDate = listing.ListedDate,
                Status = listing.Status,
                Description = listing.Description,
                RelevanceScore = _scoringService.CalculateScore(listing, request.TargetBudget)
            })
            .OrderByDescending(x => x.RelevanceScore)
            .ThenByDescending(x => x.ListedDate)
            .ThenBy(x => x.Price)
            .ThenBy(x => x.Source, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();

        int totalResults = rankedResults.Count;
        int totalPages = totalResults == 0
            ? 0
            : (int)Math.Ceiling(totalResults / (double)request.PageSize);

        IReadOnlyList<ListingSearchResult> pageItems = request.Page > totalPages
            ? Array.Empty<ListingSearchResult>()
            : rankedResults
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

        _logger.LogInformation(
            "Listing search returned {TotalResults} matches. Page {Page}/{TotalPages}, PageSize {PageSize}",
            totalResults,
            request.Page,
            totalPages,
            request.PageSize);

        return new PagedSearchResponse
        {
            Items = pageItems,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalResults = totalResults,
            TotalPages = totalPages
        };
    }
}
