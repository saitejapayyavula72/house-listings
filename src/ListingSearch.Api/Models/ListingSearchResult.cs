namespace ListingSearch.Api.Models;

/// <summary>
/// Listing returned by the search endpoint with its calculated relevance score.
/// </summary>
public sealed class ListingSearchResult
{
    public string Id { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string Zip { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Bedrooms { get; init; }
    public decimal Bathrooms { get; init; }
    public int Sqft { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string ListedDate { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public double RelevanceScore { get; init; }
}
