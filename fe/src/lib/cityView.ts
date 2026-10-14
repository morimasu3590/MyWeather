import type { Weather } from '../api/types'
import type { Favorite } from '../state/useFavorites'
import type { WeatherState } from '../state/useWeather'
import { airQualityLabel, sampleForecast, type SampleForecast } from './sample'
import { compassDirection, formatTemperature, formatUtcTime, formatWindSpeed, humanize, type Unit } from './units'

export interface Stat {
  label: string
  value: string
}

/** Everything a city column, rail row or detail view renders, already formatted for the unit. */
export interface CityView {
  favorite: Favorite
  status: WeatherState['status']
  /** Present only when the API returned weather. */
  weather?: Weather
  temperature: string | null
  condition: string | null
  updated: string | null
  stats: Stat[]
  sample: SampleForecast
  format: (celsius: number) => string
}

const DASH = '—'

function currentTemperature(weather: Weather, unit: Unit): string {
  return `${Math.round(unit === 'F' ? weather.temperatureF : weather.temperatureC)}°`
}

export function buildCityView(favorite: Favorite, state: WeatherState | undefined, unit: Unit): CityView {
  const status = state?.status ?? 'loading'
  const weather = state?.status === 'ready' ? state.weather : undefined
  const format = (celsius: number) => formatTemperature(celsius, unit)

  const stats: Stat[] = [
    { label: 'Wind', value: weather ? `${formatWindSpeed(weather.windSpeed, unit)} ${compassDirection(weather.windDirection)}` : DASH },
    { label: 'Humidity', value: weather ? `${Math.round(weather.humidity)}%` : DASH },
    { label: 'Pressure', value: weather ? `${Math.round(weather.pressure)} hPa` : DASH },
    { label: 'Visibility', value: weather ? `${weather.visibility} km` : DASH },
    { label: 'Dew point', value: weather ? format(weather.dewPoint) : DASH },
    { label: 'Cloudiness', value: weather ? humanize(weather.cloudiness) : DASH },
  ]

  return {
    favorite,
    status,
    weather,
    temperature: weather ? currentTemperature(weather, unit) : null,
    condition: weather ? weather.weatherDescription || humanize(weather.cloudiness) : null,
    updated: weather ? formatUtcTime(weather.timeUtc) : null,
    stats,
    sample: sampleForecast(favorite.cityName, weather?.temperatureC),
    format,
  }
}

export function sampleStats(view: CityView): Stat[] {
  const { sample } = view
  return [
    { label: 'Air quality', value: `${sample.airQuality} · ${airQualityLabel(sample.airQuality)}` },
    { label: 'Sunrise', value: sample.sunrise },
    { label: 'Sunset', value: sample.sunset },
  ]
}

export function statusMessage(status: WeatherState['status']): string {
  switch (status) {
    case 'loading':
      return 'Loading weather…'
    case 'missing':
      return 'No weather data yet'
    case 'error':
      return "Couldn't load weather"
    default:
      return ''
  }
}
