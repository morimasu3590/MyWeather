using MyWeather.Domain.Model;

namespace MyWeather.Domain.Repositories;

public interface IWeatherRepository
{
    Task<Weather?> GetWeatherByCityNameAsync(string cityName, CancellationToken cancellationToken = default);
}
