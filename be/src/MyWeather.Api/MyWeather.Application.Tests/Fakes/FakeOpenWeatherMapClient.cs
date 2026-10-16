using MyWeather.Application.HttpClientWrapper;

namespace MyWeather.Application.Tests.Fakes;

// Returns the given current weather, or throws the given exception to simulate a provider failure.
public class FakeOpenWeatherMapClient(OpenWeatherMapCurrentResponseDto? current, Exception? failure = null) : IOpenWeatherMapClient
{
    public string? ReceivedCountryCode { get; private set; }
    public string? ReceivedCityName { get; private set; }
    public CancellationToken? ReceivedCancellationToken { get; private set; }
    public int CallCount { get; private set; }

    public Task<OpenWeatherMapGeoResponseDto?> GetCityGeoTagAsync(string countryCode, string cityName,
        CancellationToken cancellation = default) =>
        throw new NotImplementedException();

    public Task<OpenWeatherMapCurrentResponseDto?> GetCurrentWeatherDataAsync(string countryCode, string cityName,
        CancellationToken cancellation = default)
    {
        CallCount++;
        ReceivedCountryCode = countryCode;
        ReceivedCityName = cityName;
        ReceivedCancellationToken = cancellation;
        return failure is null ? Task.FromResult(current) : Task.FromException<OpenWeatherMapCurrentResponseDto?>(failure);
    }
}
