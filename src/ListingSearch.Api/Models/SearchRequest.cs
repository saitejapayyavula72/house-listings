using System.ComponentModel.DataAnnotations;

namespace ListingSearch.Api.Models;

/// <summary>
/// Query parameters accepted by the listing search endpoint.
/// Validation lives here so all search-input rules are visible in one place.
/// </summary>
public sealed class SearchRequest : IValidatableObject
{
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinBedrooms { get; set; }
    public string? City { get; set; }
    public string? Keyword { get; set; }

    /// <summary>
    /// Target price used by the ranking calculation.
    /// The requirement uses targetBudget as one of the ranking inputs, so it must be positive.
    /// </summary>
    public decimal TargetBudget { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 5;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinPrice.HasValue && MinPrice.Value < 0)
        {
            yield return new ValidationResult(
                "minPrice must be zero or greater.",
                new[] { nameof(MinPrice) });
        }

        if (MaxPrice.HasValue && MaxPrice.Value < 0)
        {
            yield return new ValidationResult(
                "maxPrice must be zero or greater.",
                new[] { nameof(MaxPrice) });
        }

        if (MinPrice.HasValue && MaxPrice.HasValue && MinPrice.Value > MaxPrice.Value)
        {
            yield return new ValidationResult(
                "minPrice cannot be greater than maxPrice.",
                new[] { nameof(MinPrice), nameof(MaxPrice) });
        }

        if (MinBedrooms.HasValue && MinBedrooms.Value < 0)
        {
            yield return new ValidationResult(
                "minBedrooms must be zero or greater.",
                new[] { nameof(MinBedrooms) });
        }

        if (TargetBudget <= 0)
        {
            yield return new ValidationResult(
                "targetBudget must be greater than zero.",
                new[] { nameof(TargetBudget) });
        }

        if (Page <= 0)
        {
            yield return new ValidationResult(
                "page must be greater than zero.",
                new[] { nameof(Page) });
        }

        if (PageSize <= 0)
        {
            yield return new ValidationResult(
                "pageSize must be greater than zero.",
                new[] { nameof(PageSize) });
        }
    }
}
