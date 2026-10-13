using MediatR;

namespace MyWeather.Application.WeatherForecasts.Queries;

public class GetWeatherByCityNameHandler : IRequestHandler<GetWeatherByCityNameQueryDto, GetWeatherByCityNameResponseDto?>
{
    public async Task<GetWeatherByCityNameResponseDto?> Handle(GetWeatherByCityNameQueryDto request, CancellationToken cancellationToken)
    {
        // Simulate fetching weather data for the given cityName
        var response = new GetWeatherByCityNameResponseDto(
            CityId: 1,
            CityName: request.cityName,
            Country: "Sample Country",
            TimeUtc: DateTime.UtcNow.ToString("o"),
            WindSpeed: 5.0,
            WindDirection: "NW",
            Visibility: "10 km",
            Pressure: 1013.25,
            Cloudiness: "Partly Cloudy",
            TemperatureF: 68.0,
            TemperatureC: 20.0,
            Humidity: 60,
            DewPoint: 12,
            WeatherDescription: "Clear sky"
        );

        return await Task.FromResult(response);
    }
}