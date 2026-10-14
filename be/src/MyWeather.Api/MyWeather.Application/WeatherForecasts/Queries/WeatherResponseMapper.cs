using System.Globalization;
using MyWeather.Domain.Common;
using MyWeather.Domain.Model;

namespace MyWeather.Application.WeatherForecasts.Queries;

public static class WeatherResponseMapper
{
    public static GetWeatherByCityResponseDto ToResponseDto(Weather weather) => new(
        weather.CityId,
        weather.City.Name,
        weather.City.Country.Name,
        ToUtc(weather.Date).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
        weather.WindSpeed,
        weather.WindDirection,
        weather.Visibility,
        weather.Pressure,
        weather.Cloudiness.ToString(),
        TemperatureConverter.CelsiusToFahrenheit(weather.TemperatureInC),
        weather.TemperatureInC,
        weather.Humidity,
        weather.DewPoint,
        weather.WeatherDescription);

    // SQLite returns DateTime values with Kind=Unspecified. Weather dates are stored in UTC, so
    // an Unspecified value is tagged as UTC instead of being shifted by the server's local offset.
    private static DateTime ToUtc(DateTime date) => date.Kind switch
    {
        DateTimeKind.Local => date.ToUniversalTime(),
        DateTimeKind.Unspecified => DateTime.SpecifyKind(date, DateTimeKind.Utc),
        _ => date
    };
}
