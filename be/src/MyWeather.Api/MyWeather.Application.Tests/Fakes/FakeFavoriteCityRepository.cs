using MyWeather.Domain.Repositories;
using FavoriteCityModel = MyWeather.Domain.Model.FavoriteCity;

namespace MyWeather.Application.Tests.Fakes;

public class FakeFavoriteCityRepository(IReadOnlyList<FavoriteCityModel> favoriteCities) : IFavoriteCityRepository
{
    // Mutable copy, so deletes behave like the real repository (a deleted favorite is gone afterwards).
    private readonly List<FavoriteCityModel> _favorites = [.. favoriteCities];

    public CancellationToken? ReceivedCancellationToken { get; private set; }
    public List<FavoriteCityModel> Added { get; } = [];

    public int? ReceivedId { get; private set; }

    public Task<FavoriteCityModel?> GetById(int id, CancellationToken cancellation = default)
    {
        ReceivedId = id;
        ReceivedCancellationToken = cancellation;
        return Task.FromResult(_favorites.FirstOrDefault(f => f.Id == id));
    }

    public Task<IReadOnlyList<FavoriteCityModel>> GetAllFavoriteCitiesAsync(CancellationToken cancellationToken = default)
    {
        ReceivedCancellationToken = cancellationToken;
        // A snapshot, so a later delete doesn't change a list the caller already holds.
        return Task.FromResult<IReadOnlyList<FavoriteCityModel>>(_favorites.ToList());
    }

    public Task<int> AddFavoriteCityAsync(FavoriteCityModel favoriteCity, CancellationToken cancellationToken = default)
    {
        Added.Add(favoriteCity);
        favoriteCity.Id = 1000 + Added.Count;
        return Task.FromResult(favoriteCity.Id);
    }

    public List<int> DeletedIds { get; } = [];

    public Task<bool> DeleteFavoriteCityAsync(int id, CancellationToken cancellationToken = default)
    {
        DeletedIds.Add(id);
        ReceivedCancellationToken = cancellationToken;
        return Task.FromResult(_favorites.RemoveAll(f => f.Id == id) > 0);
    }
}
