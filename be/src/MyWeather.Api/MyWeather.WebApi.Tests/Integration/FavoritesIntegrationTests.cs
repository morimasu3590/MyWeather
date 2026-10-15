using System.Net;
using System.Net.Http.Json;
using MyWeather.Application.FavoriteCity.Commands;
using MyWeather.Application.FavoriteCity.Queries;
using MyWeather.Application.HttpClientWrapper;
using MyWeather.WebApi.Tests.Infrastructure;

namespace MyWeather.WebApi.Tests.Integration;

// Each test gets a fresh database, because favorites are limited to three and must be unique per city.
public class FavoritesIntegrationTests : IDisposable
{
    private static readonly DateTime Noon = new(2026, 3, 4, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteApiFactory _factory = new();
    private readonly HttpClient _client;

    public FavoritesIntegrationTests() => _client = _factory.CreateClient();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Create_ThenFollowLocation_ReturnsFavoriteWithNotesAndWeather()
    {
        var tokyo = await _factory.GetCityAsync("Tokyo");
        _factory.OpenWeatherMap.SetCurrentWeather("Tokyo", 20);

        var created = await _client.PostAsJsonAsync("/favorites", new { cityId = tokyo.Id, notes = "Sushi" });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var body = await created.Content.ReadFromJsonAsync<CreateFavoriteCityCommandResponseDto>();
        Assert.Equal($"/favorites/{body!.Id}", created.Headers.Location?.OriginalString);

        var favorite = await _client.GetFromJsonAsync<GetFavoriteCityByIdResponseDto>(created.Headers.Location);
        Assert.Equal((body.Id, "Sushi"), (favorite!.Id, favorite.Notes));
        Assert.Equal(("Tokyo", 20.0), (favorite.Weather!.CityName, favorite.Weather.TemperatureC));
    }

    [Fact]
    public async Task Create_ThenGetAll_ListsTheFavorite()
    {
        var berlin = await _factory.GetCityAsync("Berlin");
        await _factory.AddWeatherAsync(berlin.Id, Noon, 8);

        await _client.PostAsJsonAsync("/favorites", new { cityId = berlin.Id, notes = "Museums" });
        var favorites = await _client.GetFromJsonAsync<GetAllFavoriteCitiesQueryResponseDto[]>("/favorites");

        var favorite = Assert.Single(favorites!);
        Assert.Equal(("Museums", "Berlin"), (favorite.Notes, favorite.Weather.CityName));
    }

    [Fact]
    public async Task GetById_AsksOpenWeatherMapForTheFavoritesCityNotTheCityWithTheFavoritesId()
    {
        // Seeded city ids start at 1, so this favorite (id 1) shares an id with Jakarta, not with Tokyo.
        var tokyo = await _factory.GetCityAsync("Tokyo");
        _factory.OpenWeatherMap.SetCurrentWeather("Jakarta", 31);
        _factory.OpenWeatherMap.SetCurrentWeather("Tokyo", 20);
        var favorite = await _factory.AddFavoriteAsync(tokyo.Id, "Sushi");
        Assert.NotEqual(tokyo.Id, favorite.Id);

        var result = await _client.GetFromJsonAsync<GetFavoriteCityByIdResponseDto>($"/favorites/{favorite.Id}");

        Assert.Equal((tokyo.Id, "Tokyo", "Japan", 20.0),
            (result!.Weather!.CityId, result.Weather.CityName, result.Weather.Country, result.Weather.TemperatureC));
        Assert.Equal(("JP", "Tokyo"), Assert.Single(_factory.OpenWeatherMap.Requested));
    }

    [Fact]
    public async Task GetById_ReturnsFavoriteWithoutWeatherWhenOpenWeatherMapHasNone()
    {
        var favorite = await _factory.AddFavoriteAsync((await _factory.GetCityAsync("Paris")).Id, "Croissants");

        var result = await _client.GetFromJsonAsync<GetFavoriteCityByIdResponseDto>($"/favorites/{favorite.Id}");

        Assert.Equal("Croissants", result!.Notes);
        Assert.Null(result.Weather);
    }

    [Fact]
    public async Task GetById_Returns200WithoutWeatherWhenOpenWeatherMapFails()
    {
        var favorite = await _factory.AddFavoriteAsync((await _factory.GetCityAsync("Tokyo")).Id, "Sushi");
        _factory.OpenWeatherMap.FailWith("Tokyo", new WeatherProviderException("OpenWeatherMap current weather failed.",
            new HttpRequestException("Too Many Requests"), HttpStatusCode.TooManyRequests));

        var response = await _client.GetAsync($"/favorites/{favorite.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetFavoriteCityByIdResponseDto>();
        Assert.Equal((favorite.Id, "Sushi"), (result!.Id, result.Notes));
        Assert.Null(result.Weather);
    }

    [Fact]
    public async Task GetById_Returns404ForUnknownFavorite()
    {
        var response = await _client.GetAsync("/favorites/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyListWhenNoFavorites()
    {
        var favorites = await _client.GetFromJsonAsync<GetAllFavoriteCitiesQueryResponseDto[]>("/favorites");

        Assert.Empty(favorites!);
    }

    // Known gap: CreateFavoriteCityHandler throws ArgumentException / InvalidOperationException for
    // business-rule failures, and the API has no exception-to-status mapping. A real host answers 500;
    // TestServer rethrows the unhandled exception to the caller instead, which is what is asserted here.
    // When error mapping is added, change this to expect 404 / 409 / 400 responses respectively.
    [Fact]
    public async Task Create_BusinessRuleFailuresAreCurrentlyUnhandled()
    {
        var cityIds = new List<int>();
        foreach (var name in new[] { "Tokyo", "Berlin", "Paris", "London" })
        {
            cityIds.Add((await _factory.GetCityAsync(name)).Id);
        }

        await _factory.AddFavoriteAsync(cityIds[0]);
        await _factory.AddFavoriteAsync(cityIds[1]);
        await _factory.AddFavoriteAsync(cityIds[2]);

        var unknownCity = await Assert.ThrowsAsync<ArgumentException>(() =>
            _client.PostAsJsonAsync("/favorites", new { cityId = 9999, notes = "" }));
        var duplicate = await Assert.ThrowsAsync<ArgumentException>(() =>
            _client.PostAsJsonAsync("/favorites", new { cityId = cityIds[0], notes = "" }));
        var fourth = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _client.PostAsJsonAsync("/favorites", new { cityId = cityIds[3], notes = "" }));

        Assert.Contains("does not exist", unknownCity.Message);
        Assert.Contains("already a favorite", duplicate.Message);
        Assert.Contains("more than 3", fourth.Message);
    }
}
