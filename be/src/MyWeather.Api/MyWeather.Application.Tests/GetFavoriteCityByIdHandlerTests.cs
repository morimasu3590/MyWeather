using System.Net;
using MyWeather.Application.FavoriteCity.Queries;
using MyWeather.Application.HttpClientWrapper;
using MyWeather.Application.Tests.Fakes;
using MyWeather.Domain;
using MyWeather.Domain.Model;
using FavoriteCityModel = MyWeather.Domain.Model.FavoriteCity;

namespace MyWeather.Application.Tests;

public class GetFavoriteCityByIdHandlerTests
{
    private static readonly City London = new() { Id = 6, Name = "London", Country = new Country { Id = 6, Code = "GB", Name = "United Kingdom" } };

    // Favorite id (3) deliberately differs from the city id (6).
    private static readonly FavoriteCityModel LondonFavorite = new() { Id = 3, CityId = London.Id, City = London, Notes = "Museums" };

    private static readonly Weather StoredLondonWeather = new()
    {
        Id = 1,
        CityId = London.Id,
        City = London,
        Date = new DateTime(2026, 3, 4, 12, 0, 0, DateTimeKind.Utc),
        TemperatureInC = 9,
        WindSpeed = 5.5,
        Cloudiness = EnumCloudiness.PartlyCloudy,
        WeatherDescription = "Partly cloudy"
    };

    // No stored weather, so the handler falls back to OpenWeatherMap.
    private static GetFavoriteCityByIdHandler HandlerWithoutStoredWeather(FakeOpenWeatherMapClient client) =>
        new(new FakeFavoriteCityRepository([LondonFavorite]), new FakeWeatherRepository(null), client);

    [Fact]
    public async Task Handle_PrefersStoredWeatherAndSkipsApi()
    {
        var weatherRepository = new FakeWeatherRepository(StoredLondonWeather);
        var client = new FakeOpenWeatherMapClient(OpenWeatherMapSamples.London());
        var handler = new GetFavoriteCityByIdHandler(new FakeFavoriteCityRepository([LondonFavorite]), weatherRepository, client);

        var result = await handler.Handle(new GetFavoriteCityByIdQueryDto(3), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal((3, "Museums"), (result.Id, result.Notes));
        Assert.Equal((9.0, "Partly cloudy"), (result.Weather!.TemperatureC, result.Weather.WeatherDescription));
        Assert.Equal(0, client.CallCount);
    }

    [Fact]
    public async Task Handle_LooksUpStoredWeatherByCityIdNotFavoriteId()
    {
        var weatherRepository = new FakeWeatherRepository(StoredLondonWeather);
        var handler = new GetFavoriteCityByIdHandler(
            new FakeFavoriteCityRepository([LondonFavorite]), weatherRepository, new FakeOpenWeatherMapClient(null));

        await handler.Handle(new GetFavoriteCityByIdQueryDto(3), CancellationToken.None);

        Assert.Equal(London.Id, weatherRepository.ReceivedCityId);
    }

    [Fact]
    public async Task Handle_FallsBackToApiWeatherWhenNoneStored()
    {
        var handler = HandlerWithoutStoredWeather(new FakeOpenWeatherMapClient(OpenWeatherMapSamples.London()));

        var result = await handler.Handle(new GetFavoriteCityByIdQueryDto(3), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal((3, "Museums"), (result.Id, result.Notes));
        Assert.Equal((6, "London", "United Kingdom"), (result.Weather!.CityId, result.Weather.CityName, result.Weather.Country));
        Assert.Equal(14.23, result.Weather.TemperatureC);
    }

    [Fact]
    public async Task Handle_AsksApiForTheFavoritesCityAndCountryCode()
    {
        var client = new FakeOpenWeatherMapClient(OpenWeatherMapSamples.London());
        var cts = new CancellationTokenSource();

        await HandlerWithoutStoredWeather(client).Handle(new GetFavoriteCityByIdQueryDto(3), cts.Token);

        Assert.Equal(("GB", "London"), (client.ReceivedCountryCode, client.ReceivedCityName));
        Assert.Equal(cts.Token, client.ReceivedCancellationToken);
    }

    [Fact]
    public async Task Handle_ReturnsFavoriteWithoutWeatherWhenNeitherSourceHasAny()
    {
        var result = await HandlerWithoutStoredWeather(new FakeOpenWeatherMapClient(null))
            .Handle(new GetFavoriteCityByIdQueryDto(3), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Null(result.Weather);
    }

    [Fact]
    public async Task Handle_ReturnsFavoriteWithoutWeatherWhenApiFails()
    {
        var failure = new WeatherProviderException("OpenWeatherMap current weather failed.",
            new HttpRequestException("Service Unavailable"), HttpStatusCode.ServiceUnavailable);

        var result = await HandlerWithoutStoredWeather(new FakeOpenWeatherMapClient(null, failure))
            .Handle(new GetFavoriteCityByIdQueryDto(3), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal((3, "Museums"), (result.Id, result.Notes));
        Assert.Null(result.Weather);
    }

    [Fact]
    public async Task Handle_PropagatesCancellation()
    {
        var handler = HandlerWithoutStoredWeather(new FakeOpenWeatherMapClient(null, new OperationCanceledException()));

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            handler.Handle(new GetFavoriteCityByIdQueryDto(3), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_PropagatesNonProviderErrors()
    {
        // A configuration or programming error must stay loud rather than look like "no weather".
        var handler = HandlerWithoutStoredWeather(
            new FakeOpenWeatherMapClient(null, new InvalidOperationException("OpenWeather:ApiKey is not configured.")));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new GetFavoriteCityByIdQueryDto(3), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ReturnsNullAndSkipsWeatherWhenFavoriteNotFound()
    {
        var weatherRepository = new FakeWeatherRepository(StoredLondonWeather);
        var client = new FakeOpenWeatherMapClient(OpenWeatherMapSamples.London());
        var handler = new GetFavoriteCityByIdHandler(new FakeFavoriteCityRepository([LondonFavorite]), weatherRepository, client);

        var result = await handler.Handle(new GetFavoriteCityByIdQueryDto(99), CancellationToken.None);

        Assert.Null(result);
        Assert.Null(weatherRepository.ReceivedCityId);
        Assert.Equal(0, client.CallCount);
    }
}
