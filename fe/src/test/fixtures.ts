import type { City, Country, Weather } from '../api/types'

export const countries: Country[] = [
  { countryId: 1, countryCode: 'ID', countryName: 'Indonesia' },
  { countryId: 7, countryCode: 'CA', countryName: 'Canada' },
  { countryId: 14, countryCode: 'NO', countryName: 'Norway' },
]

export const citiesByCountry: Record<string, City[]> = {
  ID: [
    { cityId: 1, cityName: 'Jakarta', countryCode: 'ID', countryName: 'Indonesia' },
    { cityId: 21, cityName: 'Bandung', countryCode: 'ID', countryName: 'Indonesia' },
  ],
  CA: [{ cityId: 7, cityName: 'Ottawa', countryCode: 'CA', countryName: 'Canada' }],
  NO: [{ cityId: 14, cityName: 'Oslo', countryCode: 'NO', countryName: 'Norway' }],
}

export const osloWeather: Weather = {
  cityId: 14,
  cityName: 'Oslo',
  country: 'Norway',
  timeUtc: '2026-03-04T12:30:00Z',
  windSpeed: 14,
  windDirection: 315,
  visibility: 10,
  pressure: 1013,
  cloudiness: 'PartlyCloudy',
  temperatureF: 212,
  temperatureC: 100,
  humidity: 62,
  dewPoint: 3,
  weatherDescription: 'Partly cloudy',
}
