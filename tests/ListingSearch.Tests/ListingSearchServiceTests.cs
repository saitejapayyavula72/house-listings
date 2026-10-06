using ListingSearch.Api.Models;
using ListingSearch.Api.Services;
using Xunit;

namespace ListingSearch.Tests;

public sealed class ListingSearchServiceTests
{
    private ListingSearchService CreateSut(IEnumerable<Listing> listings)
    {
        var repository = new FakeListingRepository(listings);
        var scorer = new ListingScoringService(new FixedTodayProvider(new DateTime(2026, 10, 5)));
        return new ListingSearchService(repository, scorer, new TestLogger<ListingSearchService>());
    }

    [Fact]
    public async Task SearchAsync_WithPriceBedroomAndCityFilters_ReturnsOnlyMatchingListings()
    {
        var listings = new[]
        {
            TestData.Listing("A1", price: 450000, bedrooms: 2, city: "Springfield"),
            TestData.Listing("A2", price: 525000, bedrooms: 3, city: "Springfield"),
            TestData.Listing("A3", price: 500000, bedrooms: 4, city: "Fairfax")
        };

        var result = await CreateSut(listings).SearchAsync(new SearchRequest
        {
            MinPrice = 500000,
            MaxPrice = 550000,
            MinBedrooms = 3,
            City = " springFIELD ",
            TargetBudget = 525000,
            Page = 1,
            PageSize = 10
        }, CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal("A2", item.Id);
        Assert.Equal(1, result.TotalResults);
    }

    [Fact]
    public async Task SearchAsync_KeywordOnlyMatchesDescription_NotAddress()
    {
        var listings = new[]
        {
            TestData.Listing("A1", description: "Quiet home with large yard."),
            TestData.Listing("A2", description: "Updated kitchen and garage.")
        };

        var result = await CreateSut(listings).SearchAsync(new SearchRequest
        {
            Keyword = "main",
            TargetBudget = 450000,
            Page = 1,
            PageSize = 10
        }, CancellationToken.None);

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task SearchAsync_KeywordMatchIsCaseInsensitiveAndTrimmed()
    {
        var listings = new[]
        {
            TestData.Listing("A1", description: "Bright TOP-FLOOR condo.")
        };

        var result = await CreateSut(listings).SearchAsync(new SearchRequest
        {
            Keyword = "  top-floor  ",
            TargetBudget = 450000,
            Page = 1,
            PageSize = 10
        }, CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal("A1", item.Id);
    }

    [Fact]
    public async Task SearchAsync_WhenThereAreNoMatches_ReturnsEmptyPageWithoutError()
    {
        var result = await CreateSut(new[] { TestData.Listing(city: "Springfield") }).SearchAsync(new SearchRequest
        {
            City = "DoesNotExist",
            TargetBudget = 450000,
            Page = 1,
            PageSize = 5
        }, CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalResults);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public async Task SearchAsync_WhenPageIsBeyondLastPage_ReturnsEmptyItemsAndKeepsPagingMetadata()
    {
        var listings = Enumerable.Range(1, 6)
            .Select(i => TestData.Listing($"A{i}", price: 400000 + i * 1000, listedDate: "2026-10-05"));

        var result = await CreateSut(listings).SearchAsync(new SearchRequest
        {
            TargetBudget = 405000,
            Page = 3,
            PageSize = 3
        }, CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(6, result.TotalResults);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(3, result.Page);
    }

    [Fact]
    public async Task SearchAsync_AtPaginationBoundary_ReturnsExactlyTheLastItems()
    {
        var listings = Enumerable.Range(1, 6)
            .Select(i => TestData.Listing($"A{i}", price: 400000, listedDate: $"2026-09-{i:00}"));

        var result = await CreateSut(listings).SearchAsync(new SearchRequest
        {
            TargetBudget = 400000,
            Page = 2,
            PageSize = 3
        }, CancellationToken.None);

        Assert.Equal(3, result.Items.Count);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal("A3", result.Items[0].Id);
    }

    [Fact]
    public async Task SearchAsync_WhenScoresTie_UsesDeterministicSecondarySort()
    {
        var listings = new[]
        {
            TestData.Listing("B", source: "MLS_B", price: 500000, listedDate: "2026-10-05"),
            TestData.Listing("A", source: "MLS_A", price: 500000, listedDate: "2026-10-05")
        };

        var result = await CreateSut(listings).SearchAsync(new SearchRequest
        {
            TargetBudget = 500000,
            Page = 1,
            PageSize = 10
        }, CancellationToken.None);

        Assert.Equal(new[] { "A", "B" }, result.Items.Select(x => x.Id));
        Assert.Equal(100, result.Items[0].RelevanceScore);
        Assert.Equal(100, result.Items[1].RelevanceScore);
    }

    [Fact]
    public async Task SearchAsync_PreservesMultipleSourcesForSameUnderlyingProperty()
    {
        var listings = new[]
        {
            TestData.Listing("A1", source: "MLS_A", price: 450000),
            TestData.Listing("B7", source: "MLS_B", price: 452000)
        };

        var result = await CreateSut(listings).SearchAsync(new SearchRequest
        {
            TargetBudget = 451000,
            Page = 1,
            PageSize = 10
        }, CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, x => x.Source == "MLS_A");
        Assert.Contains(result.Items, x => x.Source == "MLS_B");
    }
}
