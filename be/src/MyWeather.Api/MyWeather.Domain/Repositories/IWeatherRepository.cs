using MyWeather.Domain.Model;

namespace MyWeather.Domain.Repositories;

public interface IWeatherRepository
{
    Task<Weather?> GetWeatherByCityIdAsync(int cityId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Weather>> GetWeatherByCityIdsAsync(IEnumerable<int> cityIds, CancellationToken cancellationToken = default); 
    Task<Weather?> GetWeatherByCityNameAsync(string cityName, CancellationToken cancellationToken = default);
}
