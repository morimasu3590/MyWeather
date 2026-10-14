using System.Net;
using System.Net.Http.Json;
using MyWeather.Application.FavoriteCity.Commands;
using MyWeather.Application.FavoriteCity.Queries;
using MyWeather.Application.WeatherForecasts.Queries;
using MyWeather.WebApi.Tests.Infrastructure;

namespace MyWeather.WebApi.Tests.Endpoints;

public class FavoritesEndpointsTests : IDisposable
{
    private static readonly GetWeatherByCityResponseDto OsloWeather = new(
        10, "Oslo", "Norway", "2026-03-04T12:00:00Z", 5.5, 180, 10000, 1013, "PartlyCloudy", 212, 100, 80, 3, "Partly cloudy");

    private readonly FakeSenderApiFactory _factory = new();
    private readonly HttpClient _client;

    public FavoritesEndpointsTests() => _client = _factory.CreateClient();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetAll_Returns200WithSenderResponse()
    {
        IReadOnlyList<GetAllFavoriteCitiesQueryResponseDto> favorites = [new(3, "Fjords", OsloWeather)];
        _factory.Sender.Returns<GetAllFavoriteCitiesQueryDto>(favorites);

        var response = await _client.GetAsync("/favorites");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(favorites, await response.Content.ReadFromJsonAsync<GetAllFavoriteCitiesQueryResponseDto[]>());
    }

    [Fact]
    public async Task GetById_Returns200AndSendsId()
    {
        var favorite = new GetFavoriteCityByIdResponseDto(3, "Fjords", OsloWeather);
        _factory.Sender.Returns<GetFavoriteCityByIdQueryDto>(favorite);

        var response = await _client.GetAsync("/favorites/3");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(favorite, await response.Content.ReadFromJsonAsync<GetFavoriteCityByIdResponseDto>());
        Assert.Equal(new GetFavoriteCityByIdQueryDto(3), Assert.Single(_factory.Sender.Received));
    }

    [Fact]
    public async Task GetById_Returns404WhenFavoriteNotFound()
    {
        _factory.Sender.Returns<GetFavoriteCityByIdQueryDto>(null);

        var response = await _client.GetAsync("/favorites/3");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Returns400ForNonPositiveIdWithoutSending()
    {
        var response = await _client.GetAsync("/favorites/0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_factory.Sender.Received);
    }

    [Fact]
    public async Task GetById_NonNumericIdDoesNotMatchRoute()
    {
        var response = await _client.GetAsync("/favorites/abc");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Empty(_factory.Sender.Received);
    }

    [Fact]
    public async Task Create_BindsJsonBodyAndReturns201WithLocation()
    {
        _factory.Sender.Returns<CreateFavoriteCityCommandDto>(new CreateFavoriteCityCommandResponseDto(42));

        var response = await _client.PostAsJsonAsync("/favorites", new { cityId = 10, notes = "Fjords" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/favorites/42", response.Headers.Location?.OriginalString);
        Assert.Equal(new CreateFavoriteCityCommandResponseDto(42),
            await response.Content.ReadFromJsonAsync<CreateFavoriteCityCommandResponseDto>());
        Assert.Equal(new CreateFavoriteCityCommandDto(10, "Fjords"), Assert.Single(_factory.Sender.Received));
    }

    [Fact]
    public async Task Create_Returns400ForNonPositiveCityIdWithoutSending()
    {
        var response = await _client.PostAsJsonAsync("/favorites", new { cityId = 0, notes = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_factory.Sender.Received);
    }

    [Fact]
    public async Task Create_Returns400ForNotesLongerThan500WithoutSending()
    {
        var response = await _client.PostAsJsonAsync("/favorites", new { cityId = 10, notes = new string('a', 501) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_factory.Sender.Received);
    }
}
