# UI Enhancements

This client keeps the original API contract for filters, ranking, and pagination, while presenting results as a real estate shopping experience.

## What changed

- List/card view is the default; table view remains available from the view switch.
- Target budget is the first filter and uses presets derived from the supplied sample data, with a custom option.
- Min/max price use a dual range control instead of text boxes.
- Keyword is presented as the primary global-style search box. The backend still matches it only against `description`, exactly as required by the problem.
- Results-per-page sits beside the result list and changing it immediately requests page 1 with the current filters.
- Relevance is displayed as a visual score ring with an understandable label.
- Listing description is shown in the card because it explains why a keyword match may matter.
- Responsive layout switches the card from image-left/details-right to image-top/details-bottom on smaller screens.
- Light/dark mode is persisted in local storage.
- A map action uses the listing's latitude/longitude when those values are returned by the API.
- No third-party image service is required. Property visuals are deliberately lightweight local illustrations so the application remains runnable without external image dependencies.

## Small backend response enhancement

The original API response projected enough fields for the table but did not return `description`, `latitude`, or `longitude` even though the source model contains them. The new UI needs those existing model fields, so the backend projection should expose them.

Update `Models/ListingSearchResult.cs` with:

```csharp
public double Latitude { get; init; }
public double Longitude { get; init; }
public string Description { get; init; } = string.Empty;
```

And in `Services/ListingSearchService.cs`, copy those three existing values into the response projection:

```csharp
Latitude = listing.Latitude,
Longitude = listing.Longitude,
Description = listing.Description,
```

No new data source, model field, deduplication rule, or search rule is introduced by this change.
