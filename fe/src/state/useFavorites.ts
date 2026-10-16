import { useCallback, useEffect, useState } from 'react'
import { addFavorite, deleteFavorite, getFavorites } from '../api/client'
import type { City, FavoriteCity } from '../api/types'

/** Same limit as CreateFavoriteCityHandler on the backend. */
export const MAX_FAVORITES = 3

/** `id` is the favorite's own id (used by the API); components key favorites by `cityId`. */
export type Favorite = Pick<FavoriteCity, 'id' | 'cityId' | 'cityName' | 'countryCode' | 'countryName' | 'notes'>

/** Same limit as CreateFavoriteCityCommandDtoValidator on the backend. */
export const MAX_NOTES_LENGTH = 500

export type FavoritesStatus = 'loading' | 'ready' | 'error'

const toFavorite = ({ id, cityId, cityName, countryCode, countryName, notes }: FavoriteCity): Favorite => ({
  id,
  cityId,
  cityName,
  countryCode,
  countryName,
  notes: notes ?? '',
})

/** Favorites are stored by the API (/favorites), so they are shared by every browser using it. */
export function useFavorites() {
  const [favorites, setFavorites] = useState<Favorite[]>([])
  const [status, setStatus] = useState<FavoritesStatus>('loading')
  const [attempt, setAttempt] = useState(0)
  // The last add or remove that failed, shown until dismissed or the next change.
  const [actionError, setActionError] = useState<string | null>(null)

  useEffect(() => {
    const controller = new AbortController()
    getFavorites(controller.signal)
      .then((list) => {
        setFavorites(list.map(toFavorite))
        setStatus('ready')
      })
      .catch((error: unknown) => {
        if (controller.signal.aborted) return
        console.error('Loading favorites failed', error)
        setStatus('error')
      })
    return () => controller.abort()
  }, [attempt])

  const reload = useCallback(() => {
    setStatus('loading')
    setAttempt((n) => n + 1)
  }, [])

  /** Resolves to false when the city is already a favorite, the limit is reached, or the API call fails. */
  const add = useCallback(
    async (city: City, notes = ''): Promise<boolean> => {
      if (favorites.length >= MAX_FAVORITES || favorites.some((f) => f.cityId === city.cityId)) {
        return false
      }
      setActionError(null)
      try {
        const id = await addFavorite(city.cityId, notes)
        const { cityId, cityName, countryCode, countryName } = city
        setFavorites((current) =>
          current.some((f) => f.cityId === cityId)
            ? current
            : [...current, { id, cityId, cityName, countryCode, countryName, notes }],
        )
        return true
      } catch (error) {
        console.error(`Adding ${city.cityName} to favorites failed`, error)
        setActionError(`Couldn't add ${city.cityName} to your favorites.`)
        return false
      }
    },
    [favorites],
  )

  /** Removes the city right away and puts it back if the API call fails. */
  const remove = useCallback(
    async (cityId: number): Promise<void> => {
      const index = favorites.findIndex((f) => f.cityId === cityId)
      if (index < 0) return
      const favorite = favorites[index]
      setActionError(null)
      setFavorites((current) => current.filter((f) => f.cityId !== cityId))
      try {
        await deleteFavorite(favorite.id)
      } catch (error) {
        console.error(`Removing ${favorite.cityName} from favorites failed`, error)
        setActionError(`Couldn't remove ${favorite.cityName} from your favorites.`)
        setFavorites((current) => {
          if (current.some((f) => f.cityId === cityId)) return current
          const restored = [...current]
          restored.splice(Math.min(index, restored.length), 0, favorite)
          return restored
        })
      }
    },
    [favorites],
  )

  const dismissError = useCallback(() => setActionError(null), [])

  return {
    favorites,
    status,
    reload,
    add,
    remove,
    isFull: favorites.length >= MAX_FAVORITES,
    actionError,
    dismissError,
  }
}
