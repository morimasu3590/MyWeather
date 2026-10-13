namespace MyWeather.Application.WeatherForecasts.Queries;

public record GetWeatherByCityNameResponseDto(
    int CityId,
    string CityName,
    string Country,
    string TimeUtc,
    double WindSpeed,
    string WindDirection,
    string Visibility,
    double Pressure,
    string Cloudiness,
    double TemperatureF,
    double TemperatureC,
    int Humidity,
    int DewPoint,
    string WeatherDescription
);