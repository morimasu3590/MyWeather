using System.Net;
using System.Net.Http.Json;
using MyWeather.Application.WeatherForecasts.Queries;
using MyWeather.WebApi.Tests.Infrastructure;

namespace MyWeather.WebApi.Tests.Endpoints;

public class WeatherEndpointsTests : IDisposable
{
    private static readonly GetWeatherByCityResponseDto OsloWeather = new(
        10, "Oslo", "Norway", "2026-03-04T12:00:00Z", 5.5, 180, 10000, 1013, "PartlyCloudy", 212, 100, 80, 3, "Partly cloudy");

    private readonly FakeSenderApiFactory _factory = new();
    private readonly HttpClient _client;

    public WeatherEndpointsTests() => _client = _factory.CreateClient();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetByCityName_Returns200AndSendsCityName()
    {
        _factory.Sender.Returns<GetWeatherByCityNameQueryDto>(OsloWeather);

        var response = await _client.GetAsync("/weathers/Oslo");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(OsloWeather, await response.Content.ReadFromJsonAsync<GetWeatherByCityResponseDto>());
        Assert.Equal(new GetWeatherByCityNameQueryDto("Oslo"), Assert.Single(_factory.Sender.Received));
    }

    [Fact]
    public async Task GetByCityName_DecodesSpacesInCityName()
    {
        _factory.Sender.Returns<GetWeatherByCityNameQueryDto>(OsloWeather);

        await _client.GetAsync("/weathers/New%20York");

        Assert.Equal(new GetWeatherByCityNameQueryDto("New York"), Assert.Single(_factory.Sender.Received));
    }

    [Fact]
    public async Task GetByCityName_Returns404WhenNoWeather()
    {
        _factory.Sender.Returns<GetWeatherByCityNameQueryDto>(null);

        var response = await _client.GetAsync("/weathers/Oslo");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByCityName_Returns400ForInvalidNameWithoutSending()
    {
        var response = await _client.GetAsync("/weathers/Oslo!");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_factory.Sender.Received);
    }

    [Fact]
    public async Task GetByCityId_NumericSegmentUsesIdRoute()
    {
        _factory.Sender.Returns<GetWeatherByCityIdQueryDto>(OsloWeather);

        var response = await _client.GetAsync("/weathers/10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(OsloWeather, await response.Content.ReadFromJsonAsync<GetWeatherByCityResponseDto>());
        Assert.Equal(new GetWeatherByCityIdQueryDto(10), Assert.Single(_factory.Sender.Received));
    }

    [Fact]
    public async Task GetByCityId_Returns404WhenNoWeather()
    {
        _factory.Sender.Returns<GetWeatherByCityIdQueryDto>(null);

        var response = await _client.GetAsync("/weathers/10");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetByCityId_Returns400ForNonPositiveIdWithoutSending(int cityId)
    {
        var response = await _client.GetAsync($"/weathers/{cityId}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_factory.Sender.Received);
    }

    [Fact]
    public async Task GetByCityId_ForwardsRequestCancellationToken()
    {
        _factory.Sender.Returns<GetWeatherByCityIdQueryDto>(OsloWeather);

        await _client.GetAsync("/weathers/10");

        Assert.True(_factory.Sender.ReceivedCancellationToken?.CanBeCanceled);
    }
}
