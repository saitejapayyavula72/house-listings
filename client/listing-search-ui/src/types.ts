export interface ListingSearchResult {
  id: string;
  source: string;
  address: string;
  city: string;
  state: string;
  zip: string;
  price: number;
  bedrooms: number;
  bathrooms: number;
  sqft: number;
  latitude: number;
  longitude: number;
  listedDate: string;
  status: string;
  description: string;
  relevanceScore: number;
}

export interface PagedSearchResponse {
  items: ListingSearchResult[];
  page: number;
  pageSize: number;
  totalResults: number;
  totalPages: number;
}

export interface SearchFilters {
  minPrice: string;
  maxPrice: string;
  minBedrooms: string;
  city: string;
  keyword: string;
  targetBudget: string;
  pageSize: number;
}

export interface ApiProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
  error?: string;
  traceId?: string;
}
