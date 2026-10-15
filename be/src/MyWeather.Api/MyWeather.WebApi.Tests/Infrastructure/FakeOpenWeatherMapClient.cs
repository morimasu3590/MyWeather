using MyWeather.Application.HttpClientWrapper;

namespace MyWeather.WebApi.Tests.Infrastructure;

// Replaces the real OpenWeatherMap client in integration tests: current weather is configured per city
// name, and unknown cities return null, the same as the real client for a city the Geo API can't find.
public class FakeOpenWeatherMapClient : IOpenWeatherMapClient
{
    private readonly Dictionary<string, OpenWeatherMapCurrentResponseDto> _current = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Exception> _failures = new(StringComparer.OrdinalIgnoreCase);

    public List<(string CountryCode, string CityName)> Requested { get; } = [];

    public void SetCurrentWeather(string cityName, double celsius, string description = "overcast clouds") =>
        _current[cityName] = new OpenWeatherMapCurrentResponseDto(
            new OpenWeatherMapCoordDto(0, 0),
            [new OpenWeatherMapWeatherConditionDto(804, "Clouds", description, "04d")],
            new OpenWeatherMapMainDto(celsius, celsius, celsius, celsius, 1013, 80),
            10000,
            new OpenWeatherMapWindDto(3.5, 210, null),
            new OpenWeatherMapCloudsDto(90),
            1792029968,
            new OpenWeatherMapSysDto("XX", 1792045428, 1792084098),
            0,
            "Station");

    // Simulates OpenWeatherMap failing (down, rate-limited, bad key) for one city.
    public void FailWith(string cityName, Exception failure) => _failures[cityName] = failure;

    public Task<OpenWeatherMapGeoResponseDto?> GetCityGeoTagAsync(string countryCode, string cityName,
        CancellationToken cancellation = default) =>
        throw new NotImplementedException();

    public Task<OpenWeatherMapCurrentResponseDto?> GetCurrentWeatherDataAsync(string countryCode, string cityName,
        CancellationToken cancellation = default)
    {
        Requested.Add((countryCode, cityName));
        return _failures.TryGetValue(cityName, out var failure)
            ? Task.FromException<OpenWeatherMapCurrentResponseDto?>(failure)
            : Task.FromResult(_current.GetValueOrDefault(cityName));
    }
}
