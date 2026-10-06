import type { ApiProblemDetails, PagedSearchResponse, SearchFilters } from './types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5190';

/**
 * Calls the ASP.NET Core search endpoint. The UI only sends values that the user actually entered.
 */
export async function searchListings(filters: SearchFilters, page: number): Promise<PagedSearchResponse> {
  const params = new URLSearchParams();

  addNumberParameter(params, 'minPrice', filters.minPrice);
  addNumberParameter(params, 'maxPrice', filters.maxPrice);
  addNumberParameter(params, 'minBedrooms', filters.minBedrooms);
  addTextParameter(params, 'city', filters.city);
  addTextParameter(params, 'keyword', filters.keyword);
  addNumberParameter(params, 'targetBudget', filters.targetBudget);
  params.set('page', String(page));
  params.set('pageSize', String(filters.pageSize));

  const response = await fetch(`${API_BASE_URL}/api/listings/search?${params.toString()}`);

  if (!response.ok) {
    const problem = (await response.json().catch(() => null)) as ApiProblemDetails | null;
    throw new Error(getApiErrorMessage(response.status, problem));
  }

  return (await response.json()) as PagedSearchResponse;
}

function addNumberParameter(params: URLSearchParams, name: string, value: string): void {
  if (value.trim() !== '') {
    params.set(name, value.trim());
  }
}

function addTextParameter(params: URLSearchParams, name: string, value: string): void {
  if (value.trim() !== '') {
    params.set(name, value.trim());
  }
}

function getApiErrorMessage(status: number, problem: ApiProblemDetails | null): string {
  if (problem?.errors) {
    const firstMessage = Object.values(problem.errors).flat()[0];
    if (firstMessage) {
      return firstMessage;
    }
  }

  return problem?.detail ?? problem?.error ?? `The search request failed (HTTP ${status}).`;
}
