using MyWeather.Domain.Model;

namespace MyWeather.Domain.Repositories;

public interface ICountryRepository
{
    Task<IReadOnlyList<Country>> GetAllCountriesAsync(CancellationToken cancellationToken = default);
    Task<Country?> GetCountryByCodeAsync(string countryCode, CancellationToken cancellationToken = default);
}
