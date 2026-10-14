import { useCallback, useState } from 'react'
import type { City } from '../api/types'

/** Same limit as CreateFavoriteCityHandler on the backend. */
export const MAX_FAVORITES = 3

const STORAGE_KEY = 'myweather.favorites'

export type Favorite = Pick<City, 'cityId' | 'cityName' | 'countryCode' | 'countryName'>

// Favorites live in this browser only until the API exposes favorites endpoints.
// Storage can throw (private mode, blocked site data), so every access is guarded.
export function loadFavorites(): Favorite[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    const parsed: unknown = raw ? JSON.parse(raw) : []
    return Array.isArray(parsed) ? (parsed as Favorite[]).slice(0, MAX_FAVORITES) : []
  } catch {
    return []
  }
}

function saveFavorites(favorites: Favorite[]): void {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(favorites))
  } catch {
    // Keep working in memory when storage is unavailable.
  }
}

export function useFavorites() {
  const [favorites, setFavorites] = useState<Favorite[]>(loadFavorites)

  const update = useCallback((next: (current: Favorite[]) => Favorite[]) => {
    setFavorites((current) => {
      const result = next(current)
      saveFavorites(result)
      return result
    })
  }, [])

  /** Returns false when the city is already a favorite or the limit is reached. */
  const add = useCallback(
    (city: Favorite): boolean => {
      if (favorites.length >= MAX_FAVORITES || favorites.some((f) => f.cityId === city.cityId)) {
        return false
      }
      update((current) => [...current, city])
      return true
    },
    [favorites, update],
  )

  const remove = useCallback(
    (cityId: number) => update((current) => current.filter((f) => f.cityId !== cityId)),
    [update],
  )

  return { favorites, add, remove, isFull: favorites.length >= MAX_FAVORITES }
}
