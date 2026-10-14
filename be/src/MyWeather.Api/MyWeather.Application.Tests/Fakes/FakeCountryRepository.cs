using MyWeather.Domain.Model;
using MyWeather.Domain.Repositories;

namespace MyWeather.Application.Tests.Fakes;

public class FakeCountryRepository(IReadOnlyList<Country> countries) : ICountryRepository
{
    public CancellationToken? ReceivedCancellationToken { get; private set; }

    public Task<IReadOnlyList<Country>> GetAllCountriesAsync(CancellationToken cancellationToken = default)
    {
        ReceivedCancellationToken = cancellationToken;
        return Task.FromResult(countries);
    }
}
