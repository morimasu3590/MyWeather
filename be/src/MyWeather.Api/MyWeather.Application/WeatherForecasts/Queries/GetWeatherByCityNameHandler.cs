using System.Globalization;
using MediatR;
using MyWeather.Domain.Repositories;
using MyWeather.Domain.Common;

namespace MyWeather.Application.WeatherForecasts.Queries;

public class GetWeatherByCityNameHandler(IWeatherRepository weatherRepository) : IRequestHandler<GetWeatherByCityNameQueryDto, GetWeatherByCityNameResponseDto?>
{
    public async Task<GetWeatherByCityNameResponseDto?> Handle(GetWeatherByCityNameQueryDto request, CancellationToken cancellationToken)
    {
        // Simulate fetching weather data for the given cityName
        var response = await weatherRepository.GetWeatherByCityNameAsync(request.CityName, cancellationToken);
        return response != null ? new GetWeatherByCityNameResponseDto(
            response.CityId,
            response.City.Name,
            response.City.Country.Name,
            response.Date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
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
}