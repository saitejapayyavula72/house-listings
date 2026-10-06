using ListingSearch.Api.Services;
using Xunit;

namespace ListingSearch.Tests;

public sealed class ListingScoringServiceTests
{
    private readonly ListingScoringService _sut = new(new FixedTodayProvider(new DateTime(2026, 10, 5)));

    [Fact]
    public void CalculateScore_WhenPriceMatchesBudgetAndListingIsToday_Returns100()
    {
        var listing = TestData.Listing(price: 500000, listedDate: "2026-10-05");

        double score = _sut.CalculateScore(listing, 500000);

        Assert.Equal(100, score);
    }

    [Fact]
    public void CalculateScore_WhenListingIsOlderThanRecencyWindow_GivesZeroRecencyContribution()
    {
        var listing = TestData.Listing(price: 500000, listedDate: "2026-08-01");

        double score = _sut.CalculateScore(listing, 500000);

        Assert.Equal(60, score);
    }

    [Fact]
    public void CalculateScore_WhenPriceIsFarFromBudget_IsClampedToValidRange()
    {
        var listing = TestData.Listing(price: 1000000, listedDate: "2026-10-05");

        double score = _sut.CalculateScore(listing, 500000);

        Assert.Equal(40, score);
    }

    [Fact]
    public void CalculateScore_WhenListedDateCannotBeParsed_DoesNotThrowAndUsesZeroRecency()
    {
        var listing = TestData.Listing(price: 500000, listedDate: "not-a-date");

        double score = _sut.CalculateScore(listing, 500000);

        Assert.Equal(60, score);
    }

    [Fact]
    public void CalculateScore_WhenTargetBudgetIsZero_ThrowsClearException()
    {
        var listing = TestData.Listing();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => _sut.CalculateScore(listing, 0));

        Assert.Contains("greater than zero", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CalculateScore_WhenTargetBudgetIsNegative_ThrowsClearException()
    {
        var listing = TestData.Listing();

        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.CalculateScore(listing, -1));
    }
}
