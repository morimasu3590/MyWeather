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
