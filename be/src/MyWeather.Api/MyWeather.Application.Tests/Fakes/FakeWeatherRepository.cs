using MyWeather.Domain.Model;
using MyWeather.Domain.Repositories;

namespace MyWeather.Application.Tests.Fakes;

public class FakeWeatherRepository(Weather? weather) : IWeatherRepository
{
    private IReadOnlyList<Weather>? _weathers;

    public static FakeWeatherRepository WithWeathers(IReadOnlyList<Weather> weathers) =>
        new(weathers.FirstOrDefault()) { _weathers = weathers };

    public int BatchCallCount { get; private set; }

    public string? ReceivedCityName { get; private set; }
    public int? ReceivedCityId { get; private set; }
    public IReadOnlyList<int>? ReceivedCityIds { get; private set; }
    public CancellationToken? ReceivedCancellationToken { get; private set; }

    public Task<Weather?> GetWeatherByCityNameAsync(string cityName, CancellationToken cancellationToken = default)
    {
        ReceivedCityName = cityName;
        ReceivedCancellationToken = cancellationToken;
        return Task.FromResult(weather);
    }

    public Task<Weather?> GetWeatherByCityIdAsync(int cityId, CancellationToken cancellationToken = default)
    {
        ReceivedCityId = cityId;
        ReceivedCancellationToken = cancellationToken;
        return Task.FromResult(weather);
    }

    public Task<IReadOnlyList<Weather>> GetWeatherByCityIdsAsync(IEnumerable<int> cityIds, CancellationToken cancellationToken = default)
    {
        ReceivedCityIds = cityIds.ToList();
        ReceivedCancellationToken = cancellationToken;
        BatchCallCount++;
        IReadOnlyList<Weather> result = _weathers ?? (weather is null ? [] : [weather]);
        return Task.FromResult(result);
    }
}
