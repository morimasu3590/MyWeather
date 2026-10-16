using MyWeather.Domain.Model;

namespace MyWeather.Domain.Repositories;

public interface IFavoriteCityRepository
{
    Task<FavoriteCity?> GetById(int id, CancellationToken cancellation = default);
    Task<IReadOnlyList<FavoriteCity>> GetAllFavoriteCitiesAsync(CancellationToken cancellationToken = default);
    Task<int> AddFavoriteCityAsync(FavoriteCity favoriteCity, CancellationToken cancellationToken = default);
    // Returns true when a favorite with this id existed and was deleted.
    Task<bool> DeleteFavoriteCityAsync(int id, CancellationToken cancellationToken = default);
}