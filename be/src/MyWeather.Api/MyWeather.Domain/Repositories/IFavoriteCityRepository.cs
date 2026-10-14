using MyWeather.Domain.Model;

namespace MyWeather.Domain.Repositories;

public interface IFavoriteCityRepository
{
    Task<IReadOnlyList<FavoriteCity>> GetAllFavoriteCitiesAsync(CancellationToken cancellationToken = default);
    Task<int> AddFavoriteCityAsync(FavoriteCity favoriteCity, CancellationToken cancellationToken = default);
}