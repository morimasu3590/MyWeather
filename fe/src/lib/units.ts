export type Unit = 'C' | 'F'

const COMPASS = ['N', 'NE', 'E', 'SE', 'S', 'SW', 'W', 'NW'] as const

export function celsiusToFahrenheit(celsius: number): number {
  return (celsius * 9) / 5 + 32
}

/** Formats a Celsius value in the chosen unit, rounded, with a degree sign. */
export function formatTemperature(celsius: number, unit: Unit): string {
  const value = unit === 'F' ? celsiusToFahrenheit(celsius) : celsius
  return `${Math.round(value)}°`
}

export function formatWindSpeed(kmh: number, unit: Unit): string {
  return unit === 'F' ? `${Math.round(kmh * 0.621371)} mph` : `${Math.round(kmh)} km/h`
}

/** Maps a wind direction in degrees to one of eight compass points. */
export function compassDirection(degrees: number): string {
  const normalized = ((degrees % 360) + 360) % 360
  return COMPASS[Math.round(normalized / 45) % 8]
}

/** "2026-03-04T12:30:45Z" → "12:30". */
export function formatUtcTime(timeUtc: string): string {
  const date = new Date(timeUtc)
  if (Number.isNaN(date.getTime())) {
    return '--:--'
  }
  return `${String(date.getUTCHours()).padStart(2, '0')}:${String(date.getUTCMinutes()).padStart(2, '0')}`
}

/** Splits "PartlyCloudy" into "Partly cloudy". */
export function humanize(value: string): string {
  const spaced = value.replace(/([a-z])([A-Z])/g, '$1 $2').toLowerCase()
  return spaced.charAt(0).toUpperCase() + spaced.slice(1)
}
