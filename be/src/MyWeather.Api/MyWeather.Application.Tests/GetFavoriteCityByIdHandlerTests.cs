using MyWeather.Application.FavoriteCity.Queries;
using MyWeather.Application.Tests.Fakes;
using MyWeather.Domain;
using MyWeather.Domain.Model;
using FavoriteCityModel = MyWeather.Domain.Model.FavoriteCity;

namespace MyWeather.Application.Tests;

public class GetFavoriteCityByIdHandlerTests
{
    private static readonly City Oslo = new() { Id = 10, Name = "Oslo", Country = new Country { Id = 1, Code = "NO", Name = "Norway" } };

    // Favorite id (3) deliberately differs from the city id (10).
    private static readonly FavoriteCityModel OsloFavorite = new() { Id = 3, CityId = Oslo.Id, City = Oslo, Notes = "Fjords" };

    private static readonly Weather OsloWeather = new()
    {
        Id = 1,
        CityId = Oslo.Id,
        City = Oslo,
        Date = new DateTime(2026, 3, 4, 12, 0, 0, DateTimeKind.Utc),
        TemperatureInC = 100,
        WindSpeed = 5.5,
        Cloudiness = EnumCloudiness.PartlyCloudy,
        WeatherDescription = "Partly cloudy"
    };

    [Fact]
    public async Task Handle_ReturnsFavoriteWithIdNotesAndWeather()
    {
        var handler = new GetFavoriteCityByIdHandler(
            new FakeFavoriteCityRepository([OsloFavorite]), new FakeWeatherRepository(OsloWeather));

        var result = await handler.Handle(new GetFavoriteCityByIdQueryDto(3), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal((3, "Fjords"), (result.Id, result.Notes));
        Assert.Equal((10, "Oslo"), (result.Weather!.CityId, result.Weather.CityName));
    }

    [Fact]
    public async Task Handle_LoadsWeatherByCityIdOfTheFavoriteNotTheFavoriteId()
    {
        var weatherRepository = new FakeWeatherRepository(OsloWeather);
        var handler = new GetFavoriteCityByIdHandler(new FakeFavoriteCityRepository([OsloFavorite]), weatherRepository);

        await handler.Handle(new GetFavoriteCityByIdQueryDto(3), CancellationToken.None);

        Assert.Equal(Oslo.Id, weatherRepository.ReceivedCityId);
    }

    [Fact]
    public async Task Handle_ReturnsFavoriteWithoutWeatherWhenCityHasNone()
    {
        var handler = new GetFavoriteCityByIdHandler(
            new FakeFavoriteCityRepository([OsloFavorite]), new FakeWeatherRepository(null));

        var result = await handler.Handle(new GetFavoriteCityByIdQueryDto(3), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Null(result.Weather);
    }

    [Fact]
    public async Task Handle_ReturnsNullAndSkipsWeatherWhenFavoriteNotFound()
    {
        var weatherRepository = new FakeWeatherRepository(OsloWeather);
        var handler = new GetFavoriteCityByIdHandler(new FakeFavoriteCityRepository([OsloFavorite]), weatherRepository);

        var result = await handler.Handle(new GetFavoriteCityByIdQueryDto(99), CancellationToken.None);

        Assert.Null(result);
        Assert.Null(weatherRepository.ReceivedCityId);
    }
}
