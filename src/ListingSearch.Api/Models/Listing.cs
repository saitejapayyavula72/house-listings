namespace ListingSearch.Api.Models;

/// <summary>
/// Represents the listing shape supplied by the MLS feeds.
/// The model intentionally follows the problem statement without adding feed-specific fields.
/// </summary>
public sealed class Listing
{
    public string Id { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Bedrooms { get; set; }
    public decimal Bathrooms { get; set; }
    public int Sqft { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string ListedDate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
