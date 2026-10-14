using MyWeather.Domain.Model;
using MyWeather.Domain.Repositories;

namespace MyWeather.Application.Tests.Fakes;

public class FakeWeatherRepository(Weather? weather) : IWeatherRepository
{
    public string? ReceivedCityName { get; private set; }
    public CancellationToken? ReceivedCancellationToken { get; private set; }

    public Task<Weather?> GetWeatherByCityNameAsync(string cityName, CancellationToken cancellationToken = default)
    {
        ReceivedCityName = cityName;
        ReceivedCancellationToken = cancellationToken;
        return Task.FromResult(weather);
    }
}
