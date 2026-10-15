namespace MyWeather.Application.HttpClientWrapper;

public interface IOpenWeatherMapClient
{
    /// <returns>The first matching location, or null when OpenWeatherMap does not know the city.</returns>
    /// <exception cref="WeatherProviderException">OpenWeatherMap is unreachable, timed out, returned an
    /// error status or an unreadable response.</exception>
    Task<OpenWeatherMapGeoResponseDto?> GetCityGeoTagAsync(string countryCode, string cityName,
        CancellationToken cancellation = default);

    /// <returns>Current weather, or null when OpenWeatherMap does not know the city.</returns>
    /// <exception cref="WeatherProviderException">OpenWeatherMap is unreachable, timed out, returned an
    /// error status or an unreadable response.</exception>
    Task<OpenWeatherMapCurrentResponseDto?> GetCurrentWeatherDataAsync(string countryCode, string cityName
        , CancellationToken cancellation = default);
}
