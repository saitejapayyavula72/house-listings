# Listing Search Service

A small full-stack MLS listing search application built with ASP.NET Core 8 Web API, React + TypeScript, Bootstrap, and xUnit.

## What is implemented

The application supports all requirements from the exercise:

- `minPrice`
- `maxPrice`
- `minBedrooms`
- `city`
- free-text `keyword` matched against the listing `description`
- `targetBudget` used with recency to rank results
- pagination with page/pageSize
- real UI inputs and ranked results
- loading, no-results, validation, and API-error states
- deliberate validation for invalid values such as minPrice > maxPrice and pageSize <= 0
- positive and negative unit tests, including tied scores and pagination boundaries

The provided 12 records are included exactly as supplied. No deduplication is performed because the problem does not define a canonical property identity across sources; duplicate-looking MLS records are therefore treated as separate source records.

## Architecture

```text
React + TypeScript + Bootstrap
            |
            | HTTP GET /api/listings/search
            v
ASP.NET Core 8 Web API
    |
    +-- ListingsController
    |
    +-- ListingSearchService
    |      |
    |      +-- filtering
    |      +-- scoring
    |      +-- deterministic ordering
    |      +-- pagination
    |
    +-- JsonListingRepository
           |
           +-- Data/sample_listings.json
```

The repository reads the JSON file on each search rather than caching it. That keeps the demo simple and means adding or changing records in the JSON file is immediately reflected in the next search without changing the C# model or restarting the application.

## Scoring approach

The score is intentionally simple and explainable:

```text
Budget fit      = 60%
Recency         = 40%
Final score     = (budgetFit * 0.60 + recency * 0.40) * 100
```

### Budget fit

A listing priced exactly at the target budget gets the maximum budget score. The score decreases as the listing price moves farther away from the target budget and is clamped between 0 and 1.

### Recency

Recency uses the ISO `listedDate` and a 30-day window:

```text
recency = 1 - ageInDays / 30
```

The value is clamped to 0-1. Invalid source dates do not crash a search; they receive zero recency contribution. The supplied model states that `listedDate` is an ISO date, so this is defensive handling rather than a new data requirement.

### Ties

Results are ordered by:

```text
relevanceScore DESC
listedDate DESC
price ASC
source ASC
id ASC
```

The last fields make equal-score results deterministic.

## Why these technologies

React was selected for a simple, modern search UI without introducing a large frontend framework footprint. Bootstrap provides the layout and accessible controls without requiring a separate component library.

The backend uses an explicit controller/service/repository design instead of a large architecture framework. This keeps the code easy to read while still separating HTTP, search logic, scoring, and data access.

No SQL/MongoDB database was introduced because the exercise supplies a small JSON dataset and does not require persistence. The repository abstraction leaves that decision open for a future change.

## Prerequisites

- .NET 8 SDK
- Node.js 20+ (Node 22 is also fine)
- npm

## Run the backend

From the repository root:

```bash
dotnet restore
dotnet build
dotnet run --project src/ListingSearch.Api/ListingSearch.Api.csproj
```

The API runs at:

```text
http://localhost:5190
```

Search endpoint:

```text
GET http://localhost:5190/api/listings/search?targetBudget=500000&page=1&pageSize=5
```

## Run the frontend

Open a second terminal:

```bash
cd client/listing-search-ui
npm install
npm run dev
```

Open:

```text
http://localhost:5173
```

The React client defaults to `http://localhost:5190` for the API. To use another backend URL, set:

```text
VITE_API_BASE_URL=http://your-host:port
```

before starting/building the frontend.

## Run tests

From the repository root:

```bash
dotnet test
```

## Example searches

### All listings ranked around a $500k budget

```text
/api/listings/search?targetBudget=500000&page=1&pageSize=5
```

### Springfield listings with at least 3 bedrooms

```text
/api/listings/search?city=Springfield&minBedrooms=3&targetBudget=525000&page=1&pageSize=5
```

### Listings whose descriptions mention pets

```text
/api/listings/search?keyword=pets&targetBudget=450000&page=1&pageSize=5
```

## Error behavior

Invalid request values return HTTP 400 with validation details instead of crashing or silently returning an unrelated result set.

Examples:

```text
minPrice=600000&maxPrice=500000
```

```text
pageSize=0
```

```text
page=0
```

A valid request that simply has no matching listings returns HTTP 200 with an empty `items` array and `totalResults = 0`.