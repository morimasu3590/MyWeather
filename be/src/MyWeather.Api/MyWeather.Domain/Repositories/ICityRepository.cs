using MyWeather.Domain.Model;

namespace MyWeather.Domain.Repositories;

public interface ICityRepository
{
    Task<City?> GetCityByIdAsync(int cityId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<City>> GetCitiesByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default);
}
