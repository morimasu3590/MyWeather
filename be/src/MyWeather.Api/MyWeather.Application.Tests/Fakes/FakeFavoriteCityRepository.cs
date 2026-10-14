using MyWeather.Domain.Repositories;
using FavoriteCityModel = MyWeather.Domain.Model.FavoriteCity;

namespace MyWeather.Application.Tests.Fakes;

public class FakeFavoriteCityRepository(IReadOnlyList<FavoriteCityModel> favoriteCities) : IFavoriteCityRepository
{
    public CancellationToken? ReceivedCancellationToken { get; private set; }
    public List<FavoriteCityModel> Added { get; } = [];

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
