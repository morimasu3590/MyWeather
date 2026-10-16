// Shapes returned by the MyWeather API (ASP.NET Core serializes properties as camelCase).

export interface Country {
  countryId: number
  countryCode: string
  countryName: string
}

export interface City {
  cityId: number
  cityName: string
  countryCode: string
  countryName: string
}

/** A row of GET /favorites; weather is null until the city has a stored weather record. */
export interface FavoriteCity {
  id: number
  cityId: number
  cityName: string
  countryCode: string
  countryName: string
  notes: string
  weather: Weather | null
}

/**
 * GET /favorites/{id}. Only the favorite's id, notes and weather: no city fields. When no weather row is
 * stored the API asks OpenWeatherMap, and weather is null only if that fails too.
 */
export interface FavoriteCityDetail {
  id: number
  notes: string
  weather: Weather | null
}

export interface Weather {
  cityId: number
  cityName: string
  country: string
  timeUtc: string
  windSpeed: number
  windDirection: number
  visibility: number
  pressure: number
  cloudiness: string
  temperatureF: number
  temperatureC: number
  humidity: number
  dewPoint: number
  weatherDescription: string
}
