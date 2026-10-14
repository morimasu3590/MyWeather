import type { City, Country, Weather } from './types'

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

async function request<T>(path: string, signal?: AbortSignal): Promise<T | null> {
  const response = await fetch(`${BASE_URL}${path}`, { signal, headers: { Accept: 'application/json' } })
  if (response.status === 404) {
    return null
  }
  if (!response.ok) {
    throw new ApiError(response.status, `Request to ${path} failed with ${response.status}`)
  }
  return (await response.json()) as T
}

export async function getCountries(signal?: AbortSignal): Promise<Country[]> {
  return (await request<Country[]>('/countries', signal)) ?? []
}

/** Returns null when the country code is unknown. */
export function getCitiesByCountry(countryCode: string, signal?: AbortSignal): Promise<City[] | null> {
  return request<City[]>(`/countries/${encodeURIComponent(countryCode)}/cities`, signal)
}

/** Returns null when the city has no weather record yet. */
export function getWeatherByCity(cityName: string, signal?: AbortSignal): Promise<Weather | null> {
  return request<Weather>(`/weathers/${encodeURIComponent(cityName)}`, signal)
}
