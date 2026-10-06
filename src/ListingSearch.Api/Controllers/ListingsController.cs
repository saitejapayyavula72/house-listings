using ListingSearch.Api.Models;
using ListingSearch.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ListingSearch.Api.Controllers;

[ApiController]
[Route("api/listings")]
public sealed class ListingsController : ControllerBase
{
    private readonly IListingSearchService _searchService;
    private readonly ILogger<ListingsController> _logger;

    public ListingsController(IListingSearchService searchService, ILogger<ListingsController> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    /// <summary>
    /// Searches listings using the supported filters and ranks matches by budget fit and recency.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedSearchResponse>> Search(
        [FromQuery] SearchRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting listing search for City={City}, MinPrice={MinPrice}, MaxPrice={MaxPrice}, MinBedrooms={MinBedrooms}, Keyword={Keyword}, TargetBudget={TargetBudget}, Page={Page}, PageSize={PageSize}",
            request.City,
            request.MinPrice,
            request.MaxPrice,
            request.MinBedrooms,
            request.Keyword,
            request.TargetBudget,
            request.Page,
            request.PageSize);

        PagedSearchResponse response = await _searchService.SearchAsync(request, cancellationToken);
        return Ok(response);
    }
}
