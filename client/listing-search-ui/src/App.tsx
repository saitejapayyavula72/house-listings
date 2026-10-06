import { useEffect, useMemo, useState } from 'react';
import type { CSSProperties, FormEvent, ReactNode } from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import './index.css';
import { searchListings } from './api';
import type { ListingSearchResult, PagedSearchResponse, SearchFilters } from './types';

const DEFAULT_PAGE_SIZE = 5;
const MIN_PRICE_BOUND = 350000;
const MAX_PRICE_BOUND = 650000;
const PRICE_STEP = 5000;

const targetBudgetOptions = [
  { value: '400000', label: '$400K' },
  { value: '450000', label: '$450K' },
  { value: '475000', label: '$475K' },
  { value: '500000', label: '$500K' },
  { value: '525000', label: '$525K' },
  { value: '550000', label: '$550K' },
  { value: '600000', label: '$600K' },
  { value: 'custom', label: 'Custom budget' }
] as const;

const initialFilters: SearchFilters = {
  minPrice: String(MIN_PRICE_BOUND),
  maxPrice: String(MAX_PRICE_BOUND),
  minBedrooms: '',
  city: '',
  keyword: '',
  targetBudget: '500000',
  pageSize: DEFAULT_PAGE_SIZE
};

type ViewMode = 'list' | 'table';
type ThemeMode = 'light' | 'dark';

/**
 * Property-search UI. The component intentionally keeps state local because the application has one screen.
 * The API remains the source of truth for filtering, ranking, and pagination.
 */
function App() {
  const [filters, setFilters] = useState<SearchFilters>(initialFilters);
  const [results, setResults] = useState<PagedSearchResponse | null>(null);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [hasSearched, setHasSearched] = useState(false);
  const [viewMode, setViewMode] = useState<ViewMode>('list');
  const [theme, setTheme] = useState<ThemeMode>(() => {
    const savedTheme = localStorage.getItem('listing-search-theme');
    return savedTheme === 'dark' ? 'dark' : 'light';
  });

  useEffect(() => {
    document.documentElement.dataset.theme = theme;
    localStorage.setItem('listing-search-theme', theme);
  }, [theme]);

  const isCustomBudget = useMemo(
    () => !targetBudgetOptions.some((option) => option.value === filters.targetBudget),
    [filters.targetBudget]
  );

  const activeFilterCount = useMemo(() => {
    let count = 0;
    if (filters.city.trim()) count += 1;
    if (filters.keyword.trim()) count += 1;
    if (filters.minBedrooms.trim()) count += 1;
    if (Number(filters.minPrice) > MIN_PRICE_BOUND) count += 1;
    if (Number(filters.maxPrice) < MAX_PRICE_BOUND) count += 1;
    return count;
  }, [filters]);

  const canGoPrevious = page > 1;
  const canGoNext = results !== null && page < results.totalPages;

  function updateFilter<K extends keyof SearchFilters>(field: K, value: SearchFilters[K]): void {
    setFilters((current) => ({ ...current, [field]: value }));
  }

  function validateClientInput(): string {
    const minPrice = parseNumber(filters.minPrice);
    const maxPrice = parseNumber(filters.maxPrice);
    const minBedrooms = parseNumber(filters.minBedrooms);
    const targetBudget = parseNumber(filters.targetBudget);

    if (minPrice === null || maxPrice === null) {
      return 'Please provide a valid price range.';
    }

    if (minPrice < 0 || maxPrice < 0) {
      return 'Price values cannot be negative.';
    }

    if (minPrice > maxPrice) {
      return 'Minimum price cannot be greater than maximum price.';
    }

    if (minBedrooms !== null && (!Number.isInteger(minBedrooms) || minBedrooms < 0)) {
      return 'Minimum bedrooms must be a whole number of zero or greater.';
    }

    if (targetBudget === null || targetBudget <= 0) {
      return 'Target budget must be greater than zero.';
    }

    return '';
  }

  async function runSearch(requestedPage: number, currentFilters: SearchFilters): Promise<void> {
    setLoading(true);
    setError('');

    try {
      const response = await searchListings(currentFilters, requestedPage);
      setResults(response);
      setPage(requestedPage);
      setHasSearched(true);
    } catch (requestError) {
      console.error('Listing search failed.', requestError);
      setError(requestError instanceof Error ? requestError.message : 'Unable to load listings.');
      setResults(null);
      setHasSearched(true);
    } finally {
      setLoading(false);
    }
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>): void {
    event.preventDefault();

    const validationMessage = validateClientInput();
    if (validationMessage) {
      setError(validationMessage);
      setResults(null);
      setHasSearched(true);
      return;
    }

    void runSearch(1, filters);
  }

  function handleClear(): void {
    setFilters(initialFilters);
    setResults(null);
    setError('');
    setHasSearched(false);
    setPage(1);
  }

  function handlePageChange(nextPage: number): void {
    if (nextPage < 1 || (results && nextPage > results.totalPages)) {
      return;
    }

    void runSearch(nextPage, filters);
  }

  function handlePageSizeChange(value: number): void {
    const nextFilters = { ...filters, pageSize: value };
    setFilters(nextFilters);

    if (hasSearched) {
      void runSearch(1, nextFilters);
    }
  }

  function handleBudgetPreset(value: string): void {
    if (value !== 'custom') {
      updateFilter('targetBudget', value);
      return;
    }

    updateFilter('targetBudget', '');
  }

  return (
    <main className="app-shell">
      <div className="container app-container py-3 py-lg-5">
        <header className="hero-header mb-4">
          <div>
            <div className="eyebrow">MLS LISTING SEARCH</div>
            <h1 className="display-6 fw-semibold mb-2">Find a home that fits you</h1>
            <p className="hero-copy mb-0">
              Search the listings, then use budget fit and recency to surface the most relevant homes first.
            </p>
          </div>

          <button
            type="button"
            className="theme-toggle"
            onClick={() => setTheme(theme === 'light' ? 'dark' : 'light')}
            aria-label={`Switch to ${theme === 'light' ? 'dark' : 'light'} mode`}
          >
            <Icon name={theme === 'light' ? 'moon' : 'sun'} size={18} />
            <span>{theme === 'light' ? 'Dark mode' : 'Light mode'}</span>
          </button>
        </header>

        <section className="search-panel mb-4">
          <form onSubmit={handleSubmit} noValidate>
            <div className="search-panel-header">
              <div>
                <div className="section-kicker">Start with your budget</div>
                <h2 className="h5 mb-0">Search homes</h2>
              </div>
              {activeFilterCount > 0 && (
                <span className="filter-count">{activeFilterCount} filter{activeFilterCount === 1 ? '' : 's'} applied</span>
              )}
            </div>

            <div className="global-search mt-3">
              <Icon name="search" size={20} />
              <input
                id="keyword"
                type="search"
                value={filters.keyword}
                onChange={(event) => updateFilter('keyword', event.target.value)}
                placeholder="Search listings — pets, garage, schools, metro..."
                aria-label="Search listing descriptions"
              />
              {filters.keyword && (
                <button
                  className="clear-search"
                  type="button"
                  onClick={() => updateFilter('keyword', '')}
                  aria-label="Clear keyword search"
                >
                  <Icon name="close" size={16} />
                </button>
              )}
            </div>
            <div className="field-hint mt-2">Searches the listing description, including partial keyword matches.</div>

            <div className="row g-3 mt-1">
              <div className="col-12 col-lg-4">
                <label htmlFor="targetBudget" className="form-label">Target budget</label>
                <select
                  id="targetBudget"
                  className="form-select form-select-lg"
                  value={isCustomBudget ? 'custom' : filters.targetBudget}
                  onChange={(event) => handleBudgetPreset(event.target.value)}
                >
                  {targetBudgetOptions.map((option) => (
                    <option key={option.value} value={option.value}>{option.label}</option>
                  ))}
                </select>
                {isCustomBudget && (
                  <div className="custom-budget mt-2">
                    <div className="input-group">
                      <span className="input-group-text">$</span>
                      <input
                        type="number"
                        min="1"
                        step="1000"
                        className="form-control"
                        value={filters.targetBudget}
                        onChange={(event) => updateFilter('targetBudget', event.target.value)}
                        placeholder="e.g. 510000"
                        aria-label="Custom target budget"
                      />
                    </div>
                  </div>
                )}
                <div className="field-hint mt-2">The main ranking signal. Closer to this amount = higher relevance.</div>
              </div>

              <div className="col-12 col-lg-8">
                <div className="d-flex justify-content-between align-items-end mb-2">
                  <label className="form-label mb-0">Price range</label>
                  <span className="range-summary">
                    {formatCompactCurrency(Number(filters.minPrice))} – {formatCompactCurrency(Number(filters.maxPrice))}
                  </span>
                </div>

                <div className="range-control">
                  <input
                    aria-label="Minimum price"
                    type="range"
                    min={MIN_PRICE_BOUND}
                    max={MAX_PRICE_BOUND}
                    step={PRICE_STEP}
                    value={Number(filters.minPrice)}
                    onChange={(event) => {
                      const value = Number(event.target.value);
                      const currentMax = Number(filters.maxPrice);
                      updateFilter('minPrice', String(Math.min(value, currentMax)));
                    }}
                  />
                  <input
                    aria-label="Maximum price"
                    type="range"
                    min={MIN_PRICE_BOUND}
                    max={MAX_PRICE_BOUND}
                    step={PRICE_STEP}
                    value={Number(filters.maxPrice)}
                    onChange={(event) => {
                      const value = Number(event.target.value);
                      const currentMin = Number(filters.minPrice);
                      updateFilter('maxPrice', String(Math.max(value, currentMin)));
                    }}
                  />
                </div>
                <div className="range-labels">
                  <span>{formatCompactCurrency(MIN_PRICE_BOUND)}</span>
                  <span>{formatCompactCurrency(MAX_PRICE_BOUND)}</span>
                </div>
                <div className="field-hint mt-2">Use either slider to narrow the price range before searching.</div>
              </div>
            </div>

            <div className="row g-3 mt-1 align-items-end">
              <div className="col-12 col-md-4">
                <label htmlFor="minBedrooms" className="form-label">Bedrooms</label>
                <select
                  id="minBedrooms"
                  className="form-select"
                  value={filters.minBedrooms}
                  onChange={(event) => updateFilter('minBedrooms', event.target.value)}
                >
                  <option value="">Any bedrooms</option>
                  <option value="2">2+ bedrooms</option>
                  <option value="3">3+ bedrooms</option>
                  <option value="4">4+ bedrooms</option>
                </select>
              </div>

              <div className="col-12 col-md-4">
                <label htmlFor="city" className="form-label">City</label>
                <input
                  id="city"
                  className="form-control"
                  type="text"
                  value={filters.city}
                  onChange={(event) => updateFilter('city', event.target.value)}
                  placeholder="Springfield, Fairfax..."
                />
              </div>

              <div className="col-12 col-md-4 d-flex gap-2">
                <button className="btn btn-primary search-button flex-grow-1" type="submit" disabled={loading}>
                  <Icon name="search" size={18} />
                  {loading ? 'Searching…' : 'Search homes'}
                </button>
                <button className="btn btn-light clear-button" type="button" onClick={handleClear} disabled={loading}>
                  Clear
                </button>
              </div>
            </div>
          </form>
        </section>

        {error && (
          <div className="notice notice-error mb-4" role="alert">
            <div className="notice-icon"><Icon name="alert" size={18} /></div>
            <div>
              <strong>Search could not be completed.</strong>
              <div>{error}</div>
            </div>
          </div>
        )}

        <section className="results-section">
          <div className="results-toolbar mb-3">
            <div>
              <div className="section-kicker">Your matches</div>
              <h2 className="h4 mb-1">Recommended listings</h2>
              <div className="results-subtitle">
                {results
                  ? `${results.totalResults} matching listing${results.totalResults === 1 ? '' : 's'}`
                  : 'Search to see homes ranked for your budget.'}
              </div>
            </div>

            <div className="results-controls">
              <label className="page-size-control">
                <span>Per page</span>
                <select
                  value={filters.pageSize}
                  onChange={(event) => handlePageSizeChange(Number(event.target.value))}
                  disabled={loading}
                  aria-label="Results per page"
                >
                  <option value={5}>5</option>
                  <option value={10}>10</option>
                  <option value={20}>20</option>
                  <option value={50}>50</option>
                </select>
              </label>

              <div className="view-switch" role="group" aria-label="Result view">
                <button
                  type="button"
                  className={viewMode === 'list' ? 'active' : ''}
                  onClick={() => setViewMode('list')}
                  aria-label="List view"
                  title="List view"
                >
                  <Icon name="list" size={17} />
                </button>
                <button
                  type="button"
                  className={viewMode === 'table' ? 'active' : ''}
                  onClick={() => setViewMode('table')}
                  aria-label="Table view"
                  title="Table view"
                >
                  <Icon name="grid" size={17} />
                </button>
              </div>
            </div>
          </div>

          {loading && <LoadingState viewMode={viewMode} />}

          {!loading && hasSearched && results && results.items.length === 0 && (
            <div className="empty-state">
              <div className="empty-icon"><Icon name="home" size={30} /></div>
              <h3 className="h5 mb-2">No homes match these filters</h3>
              <p className="text-secondary mb-3">Try widening the price range, changing the city, or removing a keyword.</p>
              <button className="btn btn-outline-secondary" type="button" onClick={handleClear}>Reset search</button>
            </div>
          )}

          {!loading && results && results.items.length > 0 && viewMode === 'list' && (
            <div className="listing-list">
              {results.items.map((listing, index) => (
                <ListingCard key={`${listing.source}-${listing.id}`} listing={listing} rank={index === 0 && page === 1 ? 1 : undefined} />
              ))}
            </div>
          )}

          {!loading && results && results.items.length > 0 && viewMode === 'table' && (
            <ListingTable listings={results.items} />
          )}

          {!loading && results && results.items.length > 0 && (
            <Pagination page={page} totalPages={results.totalPages} canGoPrevious={canGoPrevious} canGoNext={canGoNext} onPageChange={handlePageChange} />
          )}
        </section>

        <footer className="app-footer mt-4">
          <span>Ranking: 60% target-budget fit + 40% listing recency.</span>
          <span className="footer-dot">•</span>
          <span>Coordinates are used for the map action when supplied by the API.</span>
        </footer>
      </div>
    </main>
  );
}

function ListingCard({ listing, rank }: { listing: ListingSearchResult; rank?: number }) {
  const hasCoordinates = isValidCoordinate(listing.latitude, listing.longitude);
  const mapUrl = hasCoordinates
    ? `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(`${listing.latitude},${listing.longitude}`)}`
    : '';
  const visualVariant = getVisualVariant(listing.city);

  return (
    <article className="listing-card">
      <div className={`property-visual visual-${visualVariant}`} aria-hidden="true">
        <div className="visual-sky" />
        <div className="visual-sun" />
        <div className="visual-hill visual-hill-back" />
        <div className="visual-hill visual-hill-front" />
        <div className="house-drawing">
          <div className="house-roof" />
          <div className="house-body">
            <div className="house-window window-left" />
            <div className="house-window window-right" />
            <div className="house-door" />
          </div>
          <div className="house-chimney" />
        </div>
        <div className="visual-overlay" />
        <div className="visual-topline">
          <span className={`status-pill status-${listing.status.toLowerCase()}`}>{listing.status}</span>
          <span className="source-pill">{listing.source}</span>
        </div>
        <div className="visual-features">
          <FeatureChip icon="bed" value={`${listing.bedrooms} bd`} />
          <FeatureChip icon="bath" value={`${formatBathrooms(listing.bathrooms)} ba`} />
          <FeatureChip icon="sqft" value={`${listing.sqft.toLocaleString()} ft²`} />
        </div>
      </div>

      <div className="listing-content">
        <div className="listing-heading-row">
          <div>
            {rank && <span className="top-match"><Icon name="star" size={14} /> Top match</span>}
            <h3 className="listing-address">{listing.address}</h3>
            <div className="listing-location"><Icon name="pin" size={15} /> {listing.city}, {listing.state} {listing.zip}</div>
          </div>
          <RelevanceScore score={listing.relevanceScore} />
        </div>

        <div className="listing-price-row">
          <div>
            <div className="listing-price">{formatCurrency(listing.price)}</div>
            <div className="listing-date">Listed {formatDate(listing.listedDate)}</div>
          </div>
          <div className="listing-meta-id">{listing.source} · {listing.id}</div>
        </div>

        <p className="listing-description">{listing.description || 'No description provided.'}</p>

        <div className="listing-bottom-row">
          <div className="listing-coordinate-text">
            {isValidCoordinate(listing.latitude, listing.longitude)
              ? `${listing.latitude.toFixed(4)}, ${listing.longitude.toFixed(4)}`
              : 'Location coordinates unavailable'}
          </div>
          {hasCoordinates ? (
            <a className="map-link" href={mapUrl} target="_blank" rel="noreferrer" aria-label={`Open map for ${listing.address}`}>
              <Icon name="pin" size={16} /> View map
            </a>
          ) : (
            <span className="map-link map-link-disabled">Coordinates unavailable</span>
          )}
        </div>
      </div>
    </article>
  );
}

function ListingTable({ listings }: { listings: ListingSearchResult[] }) {
  return (
    <div className="table-wrapper">
      <div className="table-responsive">
        <table className="table align-middle mb-0 modern-table">
          <thead>
            <tr>
              <th>Property</th>
              <th>Price</th>
              <th>Beds / Baths</th>
              <th>Sqft</th>
              <th>Status</th>
              <th>Listed</th>
              <th className="text-end">Relevance</th>
            </tr>
          </thead>
          <tbody>
            {listings.map((listing) => (
              <tr key={`${listing.source}-${listing.id}`}>
                <td>
                  <div className="fw-semibold">{listing.address}</div>
                  <div className="small text-secondary">{listing.city}, {listing.state} {listing.zip}</div>
                  <div className="small text-secondary">{listing.source} · {listing.id}</div>
                </td>
                <td className="fw-semibold">{formatCurrency(listing.price)}</td>
                <td>{listing.bedrooms} / {formatBathrooms(listing.bathrooms)}</td>
                <td>{listing.sqft.toLocaleString()}</td>
                <td><span className={`status-pill status-${listing.status.toLowerCase()}`}>{listing.status}</span></td>
                <td>{formatDate(listing.listedDate)}</td>
                <td className="text-end"><RelevanceScore score={listing.relevanceScore} compact /></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function Pagination({
  page,
  totalPages,
  canGoPrevious,
  canGoNext,
  onPageChange
}: {
  page: number;
  totalPages: number;
  canGoPrevious: boolean;
  canGoNext: boolean;
  onPageChange: (page: number) => void;
}) {
  return (
    <nav className="pagination-bar" aria-label="Listing result pages">
      <button className="pagination-button" type="button" disabled={!canGoPrevious} onClick={() => onPageChange(page - 1)}>
        <Icon name="chevron-left" size={16} /> Previous
      </button>
      <div className="pagination-info">Page <strong>{page}</strong> of <strong>{totalPages}</strong></div>
      <button className="pagination-button" type="button" disabled={!canGoNext} onClick={() => onPageChange(page + 1)}>
        Next <Icon name="chevron-right" size={16} />
      </button>
    </nav>
  );
}

function LoadingState({ viewMode }: { viewMode: ViewMode }) {
  if (viewMode === 'table') {
    return <div className="loading-state table-loading"><div className="spinner-border" role="status" aria-hidden="true" /><span>Finding matching listings…</span></div>;
  }

  return (
    <div className="loading-list">
      {[1, 2, 3].map((item) => (
        <div className="listing-card skeleton-card" key={item}>
          <div className="skeleton skeleton-image" />
          <div className="skeleton-body">
            <div className="skeleton skeleton-line w-35" />
            <div className="skeleton skeleton-line w-70" />
            <div className="skeleton skeleton-line w-45" />
            <div className="skeleton skeleton-line w-90" />
            <div className="skeleton skeleton-line w-60" />
          </div>
        </div>
      ))}
    </div>
  );
}

function RelevanceScore({ score, compact = false }: { score: number; compact?: boolean }) {
  const normalized = Math.max(0, Math.min(100, score));
  const scoreLabel = normalized >= 85 ? 'Excellent match' : normalized >= 70 ? 'Strong match' : normalized >= 50 ? 'Good match' : 'Closer look';

  if (compact) {
    return (
      <div className="compact-score">
        <strong>{normalized.toFixed(1)}</strong>
        <span>score</span>
      </div>
    );
  }

  return (
    <div className="relevance-box" title={`${scoreLabel}: ${normalized.toFixed(1)} out of 100`}>
      <div className="score-ring" style={{ '--score': `${normalized * 3.6}deg` } as CSSProperties}>
        <div className="score-ring-inner">
          <strong>{normalized.toFixed(0)}</strong>
          <span>/100</span>
        </div>
      </div>
      <div>
        <div className="relevance-title">{scoreLabel}</div>
        <div className="relevance-caption">Budget + recency</div>
      </div>
    </div>
  );
}

function FeatureChip({ icon, value }: { icon: IconName; value: string }) {
  return <span className="feature-chip"><Icon name={icon} size={15} /> {value}</span>;
}

type IconName = 'search' | 'moon' | 'sun' | 'close' | 'alert' | 'home' | 'list' | 'grid' | 'pin' | 'bed' | 'bath' | 'sqft' | 'star' | 'chevron-left' | 'chevron-right';

function Icon({ name, size = 20 }: { name: IconName; size?: number }) {
  const common = { width: size, height: size, viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', strokeWidth: 1.8, strokeLinecap: 'round' as const, strokeLinejoin: 'round' as const, 'aria-hidden': true };

  const content: Record<IconName, ReactNode> = {
    search: <><circle cx="11" cy="11" r="7" /><path d="m20 20-4-4" /></>,
    moon: <><path d="M20.8 15.4A8.8 8.8 0 0 1 8.6 3.2 9 9 0 1 0 20.8 15.4Z" /></>,
    sun: <><circle cx="12" cy="12" r="4" /><path d="M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4" /></>,
    close: <><path d="m6 6 12 12M18 6 6 18" /></>,
    alert: <><path d="M10.3 3.7 2.6 17a2 2 0 0 0 1.7 3h15.4a2 2 0 0 0 1.7-3L13.7 3.7a2 2 0 0 0-3.4 0Z" /><path d="M12 8v5M12 17h.01" /></>,
    home: <><path d="m3 10 9-7 9 7" /><path d="M5 9v11h14V9M9 20v-6h6v6" /></>,
    list: <><path d="M8 6h13M8 12h13M8 18h13" /><path d="M3 6h.01M3 12h.01M3 18h.01" /></>,
    grid: <><rect x="3" y="3" width="7" height="7" rx="1" /><rect x="14" y="3" width="7" height="7" rx="1" /><rect x="3" y="14" width="7" height="7" rx="1" /><rect x="14" y="14" width="7" height="7" rx="1" /></>,
    pin: <><path d="M20 10c0 5-8 11-8 11S4 15 4 10a8 8 0 1 1 16 0Z" /><circle cx="12" cy="10" r="2.5" /></>,
    bed: <><path d="M3 19v-8a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2v8M3 15h18M6 12h5a2 2 0 0 0-2-2H6v2Z" /></>,
    bath: <><path d="M4 12h16M6 12V6a2 2 0 0 1 4-1.4M4 12v2a5 5 0 0 0 5 5h6a5 5 0 0 0 5-5v-2M8 19l-1 2M16 19l1 2" /></>,
    sqft: <><path d="M8 3H3v5M16 3h5v5M8 21H3v-5M21 16v5h-5" /><path d="M3 8h5V3M21 8h-5V3M3 16h5v5M21 16h-5v5" /></>,
    star: <path d="m12 3 2.8 5.8 6.2.9-4.5 4.4 1.1 6.2-5.6-2.9-5.6 2.9 1.1-6.2L3 9.7l6.2-.9L12 3Z" />,
    'chevron-left': <path d="m15 18-6-6 6-6" />,
    'chevron-right': <path d="m9 18 6-6-6-6" />
  };

  return <svg {...common}>{content[name]}</svg>;
}

function parseNumber(value: string): number | null {
  if (value.trim() === '') return null;
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : null;
}

function formatCurrency(value: number): string {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(value);
}

function formatCompactCurrency(value: number): string {
  if (!Number.isFinite(value)) return '$—';
  return `$${Math.round(value / 1000)}K`;
}

function formatBathrooms(value: number): string {
  return Number.isInteger(value) ? String(value) : value.toFixed(1);
}

function formatDate(value: string): string {
  const date = new Date(`${value}T00:00:00`);
  if (Number.isNaN(date.getTime())) return value;

  return new Intl.DateTimeFormat('en-US', { month: 'short', day: 'numeric', year: 'numeric' }).format(date);
}

function getVisualVariant(city: string): string {
  const cityKey = city.trim().toLowerCase();
  const variants = ['spring', 'ocean', 'garden', 'sunset', 'meadow', 'stone'];
  const hash = [...cityKey].reduce((value, character) => value + character.charCodeAt(0), 0);
  return variants[hash % variants.length];
}

function isValidCoordinate(latitude: number, longitude: number): boolean {
  return Number.isFinite(latitude) && Number.isFinite(longitude) && Math.abs(latitude) <= 90 && Math.abs(longitude) <= 180;
}

export default App;
