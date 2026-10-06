using ListingSearch.Api.Models;

namespace ListingSearch.Api.Services;

public interface IListingScoringService
{
    double CalculateScore(Listing listing, decimal targetBudget);
}
