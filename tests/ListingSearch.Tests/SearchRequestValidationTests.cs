using System.ComponentModel.DataAnnotations;
using ListingSearch.Api.Models;
using Xunit;

namespace ListingSearch.Tests;

public sealed class SearchRequestValidationTests
{
    private static IList<ValidationResult> Validate(SearchRequest request)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void Validate_WhenMinPriceExceedsMaxPrice_ReturnsClearValidationError()
    {
        var results = Validate(new SearchRequest
        {
            MinPrice = 600000,
            MaxPrice = 500000,
            TargetBudget = 550000
        });

        Assert.Contains(results, x => x.ErrorMessage == "minPrice cannot be greater than maxPrice.");
    }

    [Fact]
    public void Validate_WhenPageSizeIsZero_ReturnsValidationError()
    {
        var results = Validate(new SearchRequest
        {
            TargetBudget = 500000,
            PageSize = 0
        });

        Assert.Contains(results, x => x.ErrorMessage!.Contains("pageSize", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_WhenPageSizeIsNegative_ReturnsValidationError()
    {
        var results = Validate(new SearchRequest
        {
            TargetBudget = 500000,
            PageSize = -1
        });

        Assert.Contains(results, x => x.ErrorMessage!.Contains("pageSize", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_WhenPageIsZero_ReturnsValidationError()
    {
        var results = Validate(new SearchRequest
        {
            TargetBudget = 500000,
            Page = 0
        });

        Assert.Contains(results, x => x.ErrorMessage!.Contains("page", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_WhenPageIsNegative_ReturnsValidationError()
    {
        var results = Validate(new SearchRequest
        {
            TargetBudget = 500000,
            Page = -1
        });

        Assert.Contains(results, x => x.ErrorMessage!.Contains("page", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_WhenMinPriceIsNegative_ReturnsValidationError()
    {
        var results = Validate(new SearchRequest
        {
            MinPrice = -1,
            TargetBudget = 500000
        });

        Assert.Contains(results, x => x.ErrorMessage!.Contains("minPrice", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_WhenMinBedroomsIsNegative_ReturnsValidationError()
    {
        var results = Validate(new SearchRequest
        {
            MinBedrooms = -1,
            TargetBudget = 500000
        });

        Assert.Contains(results, x => x.ErrorMessage!.Contains("minBedrooms", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_WhenTargetBudgetIsZero_ReturnsValidationError()
    {
        var results = Validate(new SearchRequest
        {
            TargetBudget = 0
        });

        Assert.Contains(results, x => x.ErrorMessage!.Contains("targetBudget", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_WhenTargetBudgetIsNegative_ReturnsValidationError()
    {
        var results = Validate(new SearchRequest
        {
            TargetBudget = -1
        });

        Assert.Contains(results, x => x.ErrorMessage!.Contains("targetBudget", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_WhenAllRequiredValuesAreValid_ReturnsNoErrors()
    {
        var results = Validate(new SearchRequest
        {
            MinPrice = 400000,
            MaxPrice = 500000,
            MinBedrooms = 2,
            City = "Springfield",
            Keyword = "pets",
            TargetBudget = 450000,
            Page = 1,
            PageSize = 5
        });

        Assert.Empty(results);
    }
}
