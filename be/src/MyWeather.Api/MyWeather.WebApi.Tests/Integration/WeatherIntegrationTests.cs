using System.Net;
using System.Net.Http.Json;
using MyWeather.Application.WeatherForecasts.Queries;
using MyWeather.WebApi.Tests.Infrastructure;

namespace MyWeather.WebApi.Tests.Integration;

public class WeatherIntegrationTests : IDisposable
{
    private static readonly DateTime Noon = new(2026, 3, 4, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteApiFactory _factory = new();
    private readonly HttpClient _client;

    public WeatherIntegrationTests() => _client = _factory.CreateClient();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetByCityName_ReturnsNewestWeatherMapped()
    {
        var tokyo = await _factory.GetCityAsync("Tokyo");
        await _factory.AddWeatherAsync(tokyo.Id, Noon.AddDays(-1), 5);
        await _factory.AddWeatherAsync(tokyo.Id, Noon, 100);

        var weather = await _client.GetFromJsonAsync<GetWeatherByCityResponseDto>("/weathers/tokyo");

        Assert.NotNull(weather);
        Assert.Equal((tokyo.Id, "Tokyo", "Japan"), (weather.CityId, weather.CityName, weather.Country));
        Assert.Equal((100, 212), (weather.TemperatureC, weather.TemperatureF));
        Assert.Equal("2026-03-04T12:00:00Z", weather.TimeUtc);
        Assert.Equal("PartlyCloudy", weather.Cloudiness);
    }

    [Fact]
    public async Task GetByCityName_HandlesMultiWordCityName()
    {
        var kualaLumpur = await _factory.GetCityAsync("Kuala Lumpur");
        await _factory.AddWeatherAsync(kualaLumpur.Id, Noon, 31);

        var weather = await _client.GetFromJsonAsync<GetWeatherByCityResponseDto>("/weathers/Kuala%20Lumpur");

        Assert.Equal("Kuala Lumpur", weather!.CityName);
    }

    [Fact]
    public async Task GetByCityId_ReturnsWeatherOfThatCity()
    {
        var tokyo = await _factory.GetCityAsync("Tokyo");
        var berlin = await _factory.GetCityAsync("Berlin");
        await _factory.AddWeatherAsync(tokyo.Id, Noon, 20);
        await _factory.AddWeatherAsync(berlin.Id, Noon, 8);

        var weather = await _client.GetFromJsonAsync<GetWeatherByCityResponseDto>($"/weathers/{berlin.Id}");

        Assert.Equal((berlin.Id, "Berlin", 8.0), (weather!.CityId, weather.CityName, weather.TemperatureC));
    }

    [Theory]
    [InlineData("/weathers/Tokyo")]   // seeded city without weather
    [InlineData("/weathers/Atlantis")] // unknown city
    [InlineData("/weathers/8")]       // seeded city id without weather
    [InlineData("/weathers/9999")]    // unknown city id
    public async Task Returns404WhenNoWeather(string url)
    {
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("/weathers/0")]
    [InlineData("/weathers/Tokyo!")]
    public async Task Returns400ForInvalidInput(string url)
    {
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
