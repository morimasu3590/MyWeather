using MyWeather.Application.FavoriteCity.Queries;
using MyWeather.Application.Tests.Fakes;
using MyWeather.Application.WeatherForecasts.Queries;
using MyWeather.Domain;
using MyWeather.Domain.Model;
using FavoriteCityModel = MyWeather.Domain.Model.FavoriteCity;

namespace MyWeather.Application.Tests;

public class GetAllFavoriteCitiesHandlerTests
{
    private static readonly City Oslo = new() { Id = 10, Name = "Oslo", Country = new Country { Id = 1, Code = "NO", Name = "Norway" } };
    private static readonly City Tokyo = new() { Id = 20, Name = "Tokyo", Country = new Country { Id = 2, Code = "JP", Name = "Japan" } };

    private static FavoriteCityModel Favorite(int id, City city, string notes = "") =>
        new() { Id = id, CityId = city.Id, City = city, Notes = notes };

    private static Weather WeatherOf(int id, City city, DateTime date, double celsius = 100) => new()
    {
        Id = id,
        CityId = city.Id,
        City = city,
        Date = date,
        TemperatureInC = celsius,
        WindSpeed = 5.5,
        Cloudiness = EnumCloudiness.PartlyCloudy,
        WeatherDescription = "Partly cloudy"
    };

    private static readonly DateTime Noon = new(2026, 3, 4, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Handle_ReturnsWeatherOfEachFavoriteWithIdAndNotesInRepositoryOrder()
    {
        var handler = new GetAllFavoriteCitiesHandler(
            new FakeFavoriteCityRepository([Favorite(1, Oslo, "Fjords"), Favorite(2, Tokyo, "Sushi")]),
            FakeWeatherRepository.WithWeathers([WeatherOf(1, Tokyo, Noon, 20), WeatherOf(2, Oslo, Noon, 100)]));

        var result = await handler.Handle(new GetAllFavoriteCitiesQueryDto(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        var oslo = result[0];
        Assert.Equal((1, "Fjords"), (oslo.Id, oslo.Notes));
        Assert.Equal((10, "Oslo", "NO", "Norway"), (oslo.CityId, oslo.CityName, oslo.CountryCode, oslo.CountryName));
        Assert.NotNull(oslo.Weather);
        Assert.Equal((10, "Oslo", "Norway"), (oslo.Weather.CityId, oslo.Weather.CityName, oslo.Weather.Country));
        Assert.Equal(100, oslo.Weather.TemperatureC);
        Assert.Equal(212, oslo.Weather.TemperatureF);
        Assert.Equal("2026-03-04T12:00:00Z", oslo.Weather.TimeUtc);
        Assert.Equal("PartlyCloudy", oslo.Weather.Cloudiness);
        Assert.Equal((2, "Sushi"), (result[1].Id, result[1].Notes));
        Assert.Equal((20, "Tokyo"), (result[1].Weather!.CityId, result[1].Weather!.CityName));
        Assert.Equal(20, result[1].Weather!.TemperatureC);
    }

    [Fact]
    public async Task Handle_UsesNewestWeatherRecordOfEachCity()
    {
        var handler = new GetAllFavoriteCitiesHandler(
            new FakeFavoriteCityRepository([Favorite(1, Oslo)]),
            FakeWeatherRepository.WithWeathers([WeatherOf(1, Oslo, Noon, 5), WeatherOf(2, Oslo, Noon.AddDays(1), 9), WeatherOf(3, Oslo, Noon.AddDays(-1), 1)]));

        var result = await handler.Handle(new GetAllFavoriteCitiesQueryDto(), CancellationToken.None);

        Assert.Equal(9, Assert.Single(result).Weather!.TemperatureC);
    }

    [Fact]
    public async Task Handle_ListsFavoritesWithoutWeatherWithNullWeather()
    {
        var handler = new GetAllFavoriteCitiesHandler(
            new FakeFavoriteCityRepository([Favorite(1, Oslo, "Fjords"), Favorite(2, Tokyo)]),
            FakeWeatherRepository.WithWeathers([WeatherOf(1, Tokyo, Noon)]));

        var result = await handler.Handle(new GetAllFavoriteCitiesQueryDto(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal((1, 10, "Oslo", "NO", "Norway", "Fjords"),
            (result[0].Id, result[0].CityId, result[0].CityName, result[0].CountryCode, result[0].CountryName, result[0].Notes));
        Assert.Null(result[0].Weather);
        Assert.Equal("Tokyo", result[1].Weather!.CityName);
    }

    [Fact]
    public async Task Handle_LoadsWeatherOfAllFavoritesWithOneRepositoryCall()
    {
        var weatherRepository = FakeWeatherRepository.WithWeathers([WeatherOf(1, Oslo, Noon), WeatherOf(2, Tokyo, Noon)]);
        var handler = new GetAllFavoriteCitiesHandler(
            new FakeFavoriteCityRepository([Favorite(1, Oslo), Favorite(2, Tokyo)]), weatherRepository);
        var cts = new CancellationTokenSource();

        await handler.Handle(new GetAllFavoriteCitiesQueryDto(), cts.Token);

        Assert.Equal(1, weatherRepository.BatchCallCount);
        Assert.Equal([10, 20], weatherRepository.ReceivedCityIds);
        Assert.Equal(cts.Token, weatherRepository.ReceivedCancellationToken);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyListWithoutQueryingWeatherWhenNoFavorites()
    {
        var weatherRepository = FakeWeatherRepository.WithWeathers([]);
        var handler = new GetAllFavoriteCitiesHandler(new FakeFavoriteCityRepository([]), weatherRepository);

        var result = await handler.Handle(new GetAllFavoriteCitiesQueryDto(), CancellationToken.None);

        Assert.Empty(result);
        Assert.Equal(0, weatherRepository.BatchCallCount);
    }
}
