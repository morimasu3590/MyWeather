import { useCallback, useEffect, useState } from 'react'
import { getWeatherByCity } from '../api/client'
import type { Weather } from '../api/types'

export type WeatherState =
  | { status: 'loading' }
  | { status: 'ready'; weather: Weather }
  | { status: 'missing' }
  | { status: 'error' }

/** Loads the current weather for each city name, keyed by name. A name with no entry yet is still loading. */
export function useWeather(cityNames: string[]) {
  const [states, setStates] = useState<Record<string, WeatherState>>({})
  const [attempt, setAttempt] = useState(0)
  const key = cityNames.join('\u0000')

  useEffect(() => {
    const names = key ? key.split('\u0000') : []
    const controller = new AbortController()

    // One request per city: these are independent HTTP calls, so running them together is safe.
    for (const name of names) {
      getWeatherByCity(name, controller.signal)
        .then((weather) => {
          setStates((current) => ({
            ...current,
            [name]: weather ? { status: 'ready', weather } : { status: 'missing' },
          }))
        })
        .catch((error: unknown) => {
          if (controller.signal.aborted) return
          console.error(`Loading weather for ${name} failed`, error)
          setStates((current) => ({ ...current, [name]: { status: 'error' } }))
        })
    }

    return () => controller.abort()
  }, [key, attempt])

  const retry = useCallback(() => {
    // Show failed cities as loading again while the retry runs.
    setStates((current) =>
      Object.fromEntries(
        Object.entries(current).map(([name, state]) => [name, state.status === 'error' ? { status: 'loading' } : state]),
      ),
    )
    setAttempt((n) => n + 1)
  }, [])

  return { states, retry }
}
