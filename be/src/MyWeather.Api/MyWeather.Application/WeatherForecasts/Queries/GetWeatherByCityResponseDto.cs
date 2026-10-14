namespace MyWeather.Application.WeatherForecasts.Queries;

public record GetWeatherByCityResponseDto(
    int CityId,
    string CityName,
    string Country,
    string TimeUtc,
    double WindSpeed,
    double WindDirection,
    double Visibility,
    double Pressure,
    string Cloudiness,
    double TemperatureF,
    double TemperatureC,
    double Humidity,
    double DewPoint,
    string WeatherDescription
);