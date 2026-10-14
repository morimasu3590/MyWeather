using MyWeather.Domain.Model;
using MyWeather.Domain.Repositories;

namespace MyWeather.Application.Tests.Fakes;

public class FakeCountryRepository(IReadOnlyList<Country> countries) : ICountryRepository
{
    public CancellationToken? ReceivedCancellationToken { get; private set; }
    public string? ReceivedCountryCode { get; private set; }

    public Task<IReadOnlyList<Country>> GetAllCountriesAsync(CancellationToken cancellationToken = default)
    {
        ReceivedCancellationToken = cancellationToken;
        return Task.FromResult(countries);
    }

    public Task<Country?> GetCountryByCodeAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        ReceivedCountryCode = countryCode;
        ReceivedCancellationToken = cancellationToken;
        return Task.FromResult(countries.FirstOrDefault(c => c.Code == countryCode));
    }
}
