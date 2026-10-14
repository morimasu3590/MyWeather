// Placeholder forecast data. The API only returns current conditions, so the parts of the design it
// cannot fill (hourly, 7-day, high/low, feels-like, air quality, sunrise/sunset) are generated here,
// deterministically from the city name, and always shown with a "Sample" tag.
// Ported from the design's gen() in "Weather Favorites.dc.html".

const CONDITIONS = ['Sunny', 'Partly cloudy', 'Cloudy', 'Light rain', 'Showers', 'Clear']
const DAY_NAMES = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']

export interface SampleHour {
  label: string
  tempC: number
  precipitation: number
  /** Bar height as a percentage of the chart, 20–95. */
  height: number
}

export interface SampleDay {
  label: string
  condition: string
  lowC: number
  highC: number
  /** Position of the range bar within the week's overall range, in percent. */
  offset: number
  width: number
}

export interface SampleForecast {
  highC: number
  lowC: number
  feelsLikeC: number
  airQuality: number
  sunrise: string
  sunset: string
  hours: SampleHour[]
  week: SampleDay[]
}

const pad = (n: number) => String(n).padStart(2, '0')

function hash(text: string): number {
  let h = 0
  for (const ch of text) {
    h = (h * 31 + ch.charCodeAt(0)) >>> 0
  }
  return h
}

export function airQualityLabel(aqi: number): string {
  return aqi <= 50 ? 'Good' : aqi <= 100 ? 'Moderate' : 'Poor'
}

/**
 * Builds the placeholder forecast around a base temperature: the real current temperature when the
 * city has weather, otherwise a temperature derived from the name.
 */
export function sampleForecast(cityName: string, baseTempC?: number, now: Date = new Date()): SampleForecast {
  const h = hash(cityName)
  const r = (k: number) => (h >> k) % 10
  const t = baseTempC ?? 6 + (h % 24)

  const hourly = [0, 1, 2, 1, 0, -1, -2, -3].map((d) => Math.round(t + d))
  const precipitation = [0, 1, 2, 3, 3, 2, 1, 0].map((d) => Math.min(90, (h % 5) * 10 + d * 5))
  const hMin = Math.min(...hourly)
  const hMax = Math.max(...hourly)
  const currentHour = now.getUTCHours()

  const days = [0, 1, 2, 3, 4, 5, 6].map((i) => ({
    lowC: Math.round(t - 5 + (r(i) % 3)),
    highC: Math.round(t + 1 + (r(i + 1) % 4)),
    condition: CONDITIONS[(h + i) % CONDITIONS.length],
  }))
  const wMin = Math.min(...days.map((d) => d.lowC))
  const wMax = Math.max(...days.map((d) => d.highC))
  const span = wMax - wMin || 1

  return {
    highC: Math.round(t + 2 + (r(2) % 3)),
    lowC: Math.round(t - 4 - (r(3) % 3)),
    feelsLikeC: Math.round(t - (r(4) % 3)),
    airQuality: 15 + (h % 70),
    sunrise: `06:${pad(h % 60)}`,
    sunset: `18:${pad((h >> 3) % 60)}`,
    hours: hourly.map((tempC, i) => ({
      label: i === 0 ? 'Now' : pad((currentHour + i) % 24),
      tempC,
      precipitation: precipitation[i],
      height: 20 + ((tempC - hMin) / (hMax - hMin || 1)) * 75,
    })),
    week: days.map((d, i) => ({
      ...d,
      label: i === 0 ? 'Today' : DAY_NAMES[(now.getUTCDay() + i) % 7],
      offset: ((d.lowC - wMin) / span) * 100,
      width: ((d.highC - d.lowC) / span) * 100,
    })),
  }
}
