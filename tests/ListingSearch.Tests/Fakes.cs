using ListingSearch.Api.Models;
using ListingSearch.Api.Repositories;
using ListingSearch.Api.Services;
using Microsoft.Extensions.Logging;

namespace ListingSearch.Tests;

internal sealed class FakeListingRepository : IListingRepository
{
    private readonly IReadOnlyList<Listing> _listings;

    public FakeListingRepository(IEnumerable<Listing> listings)
    {
        _listings = listings.ToList();
    }

    public Task<IReadOnlyList<Listing>> GetAllAsync(CancellationToken cancellationToken)
        => Task.FromResult(_listings);
}

internal sealed class FixedTodayProvider : ITodayProvider
{
    public FixedTodayProvider(DateTime today)
    {
        Today = today.Date;
    }

    public DateTime Today { get; }
}

internal sealed class TestLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}
