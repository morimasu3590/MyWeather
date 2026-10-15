import type { City, Country, FavoriteCity, FavoriteCityDetail } from './types'

// In development Vite proxies /api to the backend (see vite.config.ts).
const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api'

export class ApiError extends Error {
  readonly status: number

  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

interface RequestOptions {
  method?: 'GET' | 'POST' | 'DELETE'
  body?: unknown
  signal?: AbortSignal
}

/** Resolves to null for 404 and for responses without a body (204). */
async function request<T>(path: string, { method = 'GET', body, signal }: RequestOptions = {}): Promise<T | null> {
  const headers: Record<string, string> = { Accept: 'application/json' }
  if (body !== undefined) headers['Content-Type'] = 'application/json'

  const response = await fetch(`${BASE_URL}${path}`, {
    method,
    signal,
    headers,
    body: body === undefined ? undefined : JSON.stringify(body),
  })
  if (response.status === 404 || response.status === 204) {
    return null
  }
  if (!response.ok) {
    throw new ApiError(response.status, `${method} ${path} failed with ${response.status}`)
  }
  return (await response.json()) as T
}

export async function getCountries(signal?: AbortSignal): Promise<Country[]> {
  return (await request<Country[]>('/countries', { signal })) ?? []
}

/** Returns null when the country code is unknown. */
export function getCitiesByCountry(countryCode: string, signal?: AbortSignal): Promise<City[] | null> {
  return request<City[]>(`/countries/${encodeURIComponent(countryCode)}/cities`, { signal })
}

/** Returns null when there is no favorite with this id. */
export function getFavoriteById(id: number, signal?: AbortSignal): Promise<FavoriteCityDetail | null> {
  return request<FavoriteCityDetail>(`/favorites/${id}`, { signal })
}

export async function getFavorites(signal?: AbortSignal): Promise<FavoriteCity[]> {
  return (await request<FavoriteCity[]>('/favorites', { signal })) ?? []
}

/** Returns the new favorite's id. The API allows at most 3 favorites and one per city. */
export async function addFavorite(cityId: number, notes = ''): Promise<number> {
  const created = await request<{ id: number }>('/favorites', { method: 'POST', body: { cityId, notes } })
  if (!created) {
    throw new ApiError(404, `POST /favorites returned no favorite for city ${cityId}`)
  }
  return created.id
}

/** Resolves once the favorite is gone; a favorite that was already deleted (404) counts as gone. */
export async function deleteFavorite(id: number): Promise<void> {
  await request<never>(`/favorites/${id}`, { method: 'DELETE' })
}
