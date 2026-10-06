using System.Text.Json;
using ListingSearch.Api.Models;

namespace ListingSearch.Api.Repositories;

/// <summary>
/// Reads listings from the supplied JSON file.
/// A repository abstraction keeps file access out of the search logic so the data source
/// can later be changed without changing filtering, ranking, or pagination.
/// </summary>
public sealed class JsonListingRepository : IListingRepository
{
    private readonly string _filePath;
    private readonly ILogger<JsonListingRepository> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public JsonListingRepository(IHostEnvironment environment, ILogger<JsonListingRepository> logger)
    {
        _filePath = Path.Combine(environment.ContentRootPath, "Data", "sample_listings.json");
        _logger = logger;
    }

    public async Task<IReadOnlyList<Listing>> GetAllAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Loading listings from {FilePath}", _filePath);

        if (!File.Exists(_filePath))
        {
            _logger.LogError("Listing data file was not found at {FilePath}", _filePath);
            throw new FileNotFoundException("Listing data file was not found.", _filePath);
        }

        try
        {
            await using FileStream stream = File.OpenRead(_filePath);
            var listings = await JsonSerializer.DeserializeAsync<List<Listing>>(stream, _jsonOptions, cancellationToken);

            if (listings is null)
            {
                _logger.LogError("Listing data file {FilePath} contained no readable JSON array", _filePath);
                throw new InvalidDataException("Listing data file did not contain a readable JSON array.");
            }

            _logger.LogInformation("Loaded {Count} listings", listings.Count);
            return listings;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Listing data file {FilePath} contains invalid JSON", _filePath);
            throw new InvalidDataException("Listing data file contains invalid JSON.", ex);
        }

    }
}
