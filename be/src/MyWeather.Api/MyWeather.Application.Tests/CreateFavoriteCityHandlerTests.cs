using MyWeather.Application.FavoriteCity.Commands;
using MyWeather.Application.Tests.Fakes;
using MyWeather.Domain.Model;
using FavoriteCityModel = MyWeather.Domain.Model.FavoriteCity;

namespace MyWeather.Application.Tests;

public class CreateFavoriteCityHandlerTests
{
    private static readonly Country Norway = new() { Id = 1, Code = "NO", Name = "Norway" };
    private static readonly City Oslo = new() { Id = 10, Name = "Oslo", CountryId = 1, Country = Norway };

    private static FavoriteCityModel Favorite(int id, int cityId) => new() { Id = id, CityId = cityId };

    private static CreateFavoriteCityHandler CreateHandler(
        IReadOnlyList<FavoriteCityModel> existing, out FakeFavoriteCityRepository favoriteRepository)
    {
        favoriteRepository = new FakeFavoriteCityRepository(existing);
        return new CreateFavoriteCityHandler(favoriteRepository, new FakeCityRepository([Oslo]));
    }

    [Fact]
    public async Task Handle_AddsFavoriteAndReturnsItsId()
    {
        var handler = CreateHandler([], out var repository);

        var result = await handler.Handle(new CreateFavoriteCityCommandDto(10, "Fjords"), CancellationToken.None);

        var added = Assert.Single(repository.Added);
        Assert.Equal(10, added.CityId);
        Assert.Equal("Fjords", added.Notes);
        Assert.Equal(new CreateFavoriteCityCommandResponseDto(added.Id), result);
    }

    [Fact]
    public async Task Handle_ThrowsWhenCityDoesNotExist()
    {
        var handler = CreateHandler([], out var repository);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(new CreateFavoriteCityCommandDto(99, ""), CancellationToken.None));

        Assert.Contains("99", exception.Message);
        Assert.Empty(repository.Added);
    }

    [Fact]
    public async Task Handle_ThrowsWhenCityIsAlreadyFavorite()
    {
        var handler = CreateHandler([Favorite(1, 10)], out var repository);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(new CreateFavoriteCityCommandDto(10, ""), CancellationToken.None));

        Assert.Contains("already a favorite", exception.Message);
        Assert.Empty(repository.Added);
    }

    [Fact]
    public async Task Handle_ThrowsWhenThreeFavoritesAlreadyExist()
    {
        var handler = CreateHandler([Favorite(1, 20), Favorite(2, 21), Favorite(3, 22)], out var repository);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CreateFavoriteCityCommandDto(10, ""), CancellationToken.None));

        Assert.Empty(repository.Added);
    }

    [Fact]
    public async Task Handle_AllowsThirdFavorite()
    {
        var handler = CreateHandler([Favorite(1, 20), Favorite(2, 21)], out var repository);

        await handler.Handle(new CreateFavoriteCityCommandDto(10, ""), CancellationToken.None);

        Assert.Single(repository.Added);
    }
}
