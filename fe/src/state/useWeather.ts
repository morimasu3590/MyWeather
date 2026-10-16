import { useCallback, useEffect, useRef, useState } from 'react'
import { getFavoriteById } from '../api/client'
import type { Weather } from '../api/types'

export type WeatherState =
  | { status: 'loading' }
  | { status: 'ready'; weather: Weather }
  | { status: 'missing' }
  | { status: 'error' }

/**
 * Loads the current weather of each favorite through GET /favorites/{id}, keyed by favorite id. The API answers
 * from its stored weather or else asks OpenWeatherMap, so weather is missing only when neither has any. An id with
 * no entry yet is still loading.
 *
 * Each favorite is fetched once: adding one fetches only the new one, removing one cancels only its request, and
 * retry fetches only the ones that failed. `onFavoriteGone` is called when a favorite no longer exists on the API
 * (deleted elsewhere), so the caller can reload its list.
 */
export function useWeather(favoriteIds: number[], onFavoriteGone?: (favoriteId: number) => void) {
  const [states, setStates] = useState<Record<number, WeatherState>>({})
  const [attempt, setAttempt] = useState(0)
  // Favorites with a kept result (ready or missing). Failed ones are left out, so the next run fetches them again.
  const loaded = useRef(new Set<number>())
  const inFlight = useRef(new Map<number, AbortController>())
  const onGone = useRef(onFavoriteGone)
  const key = favoriteIds.join(',')

  useEffect(() => {
    onGone.current = onFavoriteGone
  })

  useEffect(() => {
    const ids = new Set(key ? key.split(',').map(Number) : [])

    // Forget removed favorites: SQLite can hand a deleted favorite's id to the next one added.
    for (const [id, controller] of inFlight.current) {
      if (!ids.has(id)) {
        controller.abort()
        inFlight.current.delete(id)
      }
    }
    const removed = [...loaded.current].filter((id) => !ids.has(id))
    if (removed.length > 0) {
      for (const id of removed) loaded.current.delete(id)
      setStates((current) => Object.fromEntries(Object.entries(current).filter(([id]) => ids.has(Number(id)))))
    }

    // One request per favorite: these are independent HTTP calls, so running them together is safe.
    for (const favoriteId of ids) {
      if (loaded.current.has(favoriteId) || inFlight.current.has(favoriteId)) continue
      const controller = new AbortController()
      inFlight.current.set(favoriteId, controller)

      getFavoriteById(favoriteId, controller.signal)
        .then((favorite) => {
          if (controller.signal.aborted) return
          loaded.current.add(favoriteId)
          const weather = favorite?.weather
          setStates((current) => ({
            ...current,
            [favoriteId]: weather ? { status: 'ready', weather } : { status: 'missing' },
          }))
          if (!favorite) onGone.current?.(favoriteId)
        })
        .catch((error: unknown) => {
          if (controller.signal.aborted) return
          console.error(`Loading weather for favorite ${favoriteId} failed`, error)
          setStates((current) => ({ ...current, [favoriteId]: { status: 'error' } }))
        })
        .finally(() => {
          if (inFlight.current.get(favoriteId) === controller) inFlight.current.delete(favoriteId)
        })
    }
  }, [key, attempt])

  // Cancel whatever is still loading on unmount.
  useEffect(() => {
    const requests = inFlight.current
    return () => {
      for (const controller of requests.values()) controller.abort()
      requests.clear()
    }
  }, [])

  const retry = useCallback(() => {
    // Show failed favorites as loading again while the retry runs; only those are fetched again.
    setStates((current) =>
      Object.fromEntries(
        Object.entries(current).map(([favoriteId, state]) => [
          favoriteId,
          state.status === 'error' ? { status: 'loading' } : state,
        ]),
      ),
    )
    setAttempt((n) => n + 1)
  }, [])

  return { states, retry }
}
