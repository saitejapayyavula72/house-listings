using ListingSearch.Api.Models;

namespace ListingSearch.Tests;

internal static class TestData
{
    public static Listing Listing(
        string id = "A1",
        string source = "MLS_A",
        decimal price = 450000,
        int bedrooms = 2,
        string city = "Springfield",
        string listedDate = "2026-09-01",
        string description = "Bright home near shops.")
    {
        return new Listing
        {
            Id = id,
            Source = source,
            Address = "123 Main St",
            City = city,
            State = "VA",
            Zip = "22150",
            Price = price,
            Bedrooms = bedrooms,
            Bathrooms = 1.5m,
            Sqft = 1000,
            Latitude = 38.0,
            Longitude = -77.0,
            ListedDate = listedDate,
            Status = "active",
            Description = description
        };
    }
}
