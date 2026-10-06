namespace ListingSearch.Api.Services;

/// <summary>
/// Provides today's date through a small abstraction so date-based scoring is easy to test.
/// </summary>
public sealed class SystemTodayProvider : ITodayProvider
{
    public DateTime Today => DateTime.UtcNow.Date;
}
