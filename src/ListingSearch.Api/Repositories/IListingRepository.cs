using ListingSearch.Api.Models;

namespace ListingSearch.Api.Repositories;

public interface IListingRepository
{
    Task<IReadOnlyList<Listing>> GetAllAsync(CancellationToken cancellationToken);
}
