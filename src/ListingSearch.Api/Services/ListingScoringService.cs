using System.Globalization;
using ListingSearch.Api.Models;

namespace ListingSearch.Api.Services;

/// <summary>
/// Calculates a simple, explainable relevance score.
///
/// Budget fit contributes 60% of the score and recency contributes 40%.
/// Budget fit is highest when price equals targetBudget and decreases with distance from it.
/// Recency decreases linearly over 30 days; listings older than 30 days receive zero recency points.
/// The final score is in the range 0-100 and is rounded to two decimal places.
/// </summary>
public sealed class ListingScoringService : IListingScoringService
{
    private const double BudgetWeight = 0.60;
    private const double RecencyWeight = 0.40;
    private const int RecencyWindowDays = 30;

    private readonly ITodayProvider _todayProvider;

    public ListingScoringService(ITodayProvider todayProvider)
    {
        _todayProvider = todayProvider;
    }

    public double CalculateScore(Listing listing, decimal targetBudget)
    {
        if (targetBudget <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetBudget), "Target budget must be greater than zero.");
        }

        double budgetScore = CalculateBudgetScore(listing.Price, targetBudget);
        double recencyScore = CalculateRecencyScore(listing.ListedDate);

        double finalScore = ((budgetScore * BudgetWeight) + (recencyScore * RecencyWeight)) * 100;
        return Math.Round(finalScore, 2, MidpointRounding.AwayFromZero);
    }

    private static double CalculateBudgetScore(decimal listingPrice, decimal targetBudget)
    {
        decimal differenceRatio = Math.Abs(listingPrice - targetBudget) / targetBudget;
        return (double)Math.Clamp(1m - differenceRatio, 0m, 1m);
    }

    private double CalculateRecencyScore(string listedDate)
    {
        if (!DateTime.TryParseExact(
                listedDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime date))
        {
            // The model says ISO date, but bad source data should not crash a search.
            return 0;
        }

        int ageInDays = Math.Max(0, (_todayProvider.Today - date.Date).Days);
        return Math.Clamp(1d - (ageInDays / (double)RecencyWindowDays), 0d, 1d);
    }
}
