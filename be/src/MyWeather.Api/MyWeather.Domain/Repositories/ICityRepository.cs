using MyWeather.Domain.Model;

namespace MyWeather.Domain.Repositories;

public interface ICityRepository
{
    Task<IReadOnlyList<City>> GetCitiesByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default);
}
