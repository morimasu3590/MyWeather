using System.Globalization;
using MediatR;
using MyWeather.Domain.Repositories;
using MyWeather.Domain.Common;

namespace MyWeather.Application.WeatherForecasts.Queries;

public class GetWeatherByCityNameHandler(IWeatherRepository weatherRepository) : IRequestHandler<GetWeatherByCityNameQueryDto, GetWeatherByCityNameResponseDto?>
{
    public async Task<GetWeatherByCityNameResponseDto?> Handle(GetWeatherByCityNameQueryDto request, CancellationToken cancellationToken)
    {
        var response = await weatherRepository.GetWeatherByCityNameAsync(request.CityName, cancellationToken);
        return response != null ? new GetWeatherByCityNameResponseDto(
            response.CityId,
            response.City.Name,
            response.City.Country.Name,
            ToUtc(response.Date).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
            response.WindSpeed,
            response.WindDirection,
            response.Visibility,
            response.Pressure,
            response.Cloudiness.ToString(),
            TemperatureConverter.CelsiusToFahrenheit(response.TemperatureInC),
            response.TemperatureInC,
            response.Humidity,
            response.DewPoint,
            response.WeatherDescription) : null;
    }

    // SQLite returns DateTime values with Kind=Unspecified. Weather dates are stored in UTC, so
    // an Unspecified value is tagged as UTC instead of being shifted by the server's local offset.
    private static DateTime ToUtc(DateTime date) => date.Kind switch
    {
        DateTimeKind.Local => date.ToUniversalTime(),
        DateTimeKind.Unspecified => DateTime.SpecifyKind(date, DateTimeKind.Utc),
        _ => date
    };
}
