using MyWeather.Domain.Model;
using MyWeather.Domain.Repositories;

namespace MyWeather.Application.Tests.Fakes;

public class FakeCityRepository(IReadOnlyList<City> cities) : ICityRepository
{
    public string? ReceivedCountryCode { get; private set; }
    public CancellationToken? ReceivedCancellationToken { get; private set; }

    public Task<IReadOnlyList<City>> GetCitiesByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        ReceivedCountryCode = countryCode;
        ReceivedCancellationToken = cancellationToken;
        return Task.FromResult(cities);
    }

    public Task<City?> GetCityByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(cities.FirstOrDefault(c => c.Id == id));
    }
}
