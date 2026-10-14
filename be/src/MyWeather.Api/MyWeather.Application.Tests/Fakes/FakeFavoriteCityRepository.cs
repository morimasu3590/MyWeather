using MyWeather.Domain.Repositories;
using FavoriteCityModel = MyWeather.Domain.Model.FavoriteCity;

namespace MyWeather.Application.Tests.Fakes;

public class FakeFavoriteCityRepository(IReadOnlyList<FavoriteCityModel> favoriteCities) : IFavoriteCityRepository
{
    public CancellationToken? ReceivedCancellationToken { get; private set; }
    public List<FavoriteCityModel> Added { get; } = [];

    public int? ReceivedId { get; private set; }

    public Task<FavoriteCityModel?> GetById(int id, CancellationToken cancellation = default)
    {
        ReceivedId = id;
        ReceivedCancellationToken = cancellation;
        return Task.FromResult(favoriteCities.FirstOrDefault(f => f.Id == id));
    }

    public Task<IReadOnlyList<FavoriteCityModel>> GetAllFavoriteCitiesAsync(CancellationToken cancellationToken = default)
    {
        ReceivedCancellationToken = cancellationToken;
        return Task.FromResult(favoriteCities);
    }

    public Task<int> AddFavoriteCityAsync(FavoriteCityModel favoriteCity, CancellationToken cancellationToken = default)
    {
        Added.Add(favoriteCity);
        favoriteCity.Id = 1000 + Added.Count;
        return Task.FromResult(favoriteCity.Id);
    }
}
