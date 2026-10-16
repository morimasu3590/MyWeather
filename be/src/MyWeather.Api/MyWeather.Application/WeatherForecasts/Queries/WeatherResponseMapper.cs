using System.Globalization;
using MyWeather.Application.HttpClientWrapper;
using MyWeather.Domain.Common;
using MyWeather.Domain.Model;

namespace MyWeather.Application.WeatherForecasts.Queries;

public static class WeatherResponseMapper
{
    // City and country come from the database: the API's own "name" is the nearest station area.
    public static GetWeatherByCityResponseDto ToResponseDto(City city, Country country,
        OpenWeatherMapCurrentResponseDto responseDto)
    {
        var isDetailExists = responseDto.Weather.Count > 0;
        return new GetWeatherByCityResponseDto(
            city.Id,
            city.Name,
            country.Name,
            ToUtc(responseDto.Dt).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
            responseDto.Wind.Speed,
            responseDto.Wind.Deg,
            responseDto.Visibility,
            responseDto.Main.Pressure,
            isDetailExists ? responseDto.Weather[0].Main : string.Empty,
            TemperatureConverter.CelsiusToFahrenheit(responseDto.Main.Temp),
            responseDto.Main.Temp,
            responseDto.Main.Humidity,
            DewPointCelsius(responseDto.Main.Temp, responseDto.Main.Humidity),
            isDetailExists ? responseDto.Weather[0].Description : string.Empty
        );
    }

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

    // OpenWeatherMap timestamps (dt, sunrise, sunset) are Unix seconds; the result has Kind=Utc.
    private static DateTime ToUtc(long unixSeconds) => DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime;

    // The Current Weather API has no dew point, so it is derived from temperature and relative humidity
    // with the Magnus formula (about ±0.4 °C for normal weather). Non-positive humidity would need ln(0).
    private static double DewPointCelsius(double temperatureC, double relativeHumidity)
    {
        if (relativeHumidity <= 0)
        {
            return temperatureC;
        }

        const double b = 17.62, c = 243.12;
        var gamma = Math.Log(relativeHumidity / 100) + b * temperatureC / (c + temperatureC);
        return Math.Round(c * gamma / (b - gamma), 2);
    }
}
