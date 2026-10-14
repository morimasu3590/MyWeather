namespace MyWeather.Application.HttpClientWrapper;

public interface IOpenWeatherMapClient
{
    Task<OpenWeatherMapGeoResponseDto?> GetCityGeoTagAsync(string countryCode, string cityName,
        CancellationToken cancellation = default);

    Task<OpenWeatherMapCurrentResponseDto?> GetCurrentWeatherDataAsync(string countryCode, string cityName
        , CancellationToken cancellation = default);
}