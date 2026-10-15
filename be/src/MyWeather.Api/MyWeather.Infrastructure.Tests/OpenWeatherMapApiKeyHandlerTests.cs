using MyWeather.Infrastructure.OpenWeatherMap;

namespace MyWeather.Infrastructure.Tests;

public class OpenWeatherMapApiKeyHandlerTests
{
    private readonly StubHttpMessageHandler _stub = new();

    private HttpClient ClientWithKey(string apiKey) =>
        new(new OpenWeatherMapApiKeyHandler(apiKey) { InnerHandler = _stub })
        {
            BaseAddress = new Uri("https://api.openweathermap.test")
        };

    [Fact]
    public async Task AppendsAppIdToExistingQuery()
    {
        await ClientWithKey("KEY").GetAsync("/geo/1.0/direct?q=Oslo,NO&limit=5");

        Assert.Equal("?q=Oslo,NO&limit=5&appid=KEY", Assert.Single(_stub.RequestUris).Query);
    }

    [Fact]
    public async Task AddsAppIdQueryWhenThereIsNone()
    {
        await ClientWithKey("KEY").GetAsync("/data/4.0/onecall/current");

        var uri = Assert.Single(_stub.RequestUris);
        Assert.Equal("/data/4.0/onecall/current", uri.AbsolutePath);
        Assert.Equal("?appid=KEY", uri.Query);
    }

    [Fact]
    public async Task EscapesTheKey()
    {
        await ClientWithKey("a b&c").GetAsync("/geo/1.0/direct?q=Oslo");

        Assert.EndsWith("&appid=a%20b%26c", Assert.Single(_stub.RequestUris).Query);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task MissingKey_ThrowsWithoutSending(string apiKey)
    {
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ClientWithKey(apiKey).GetAsync("/geo/1.0/direct?q=Oslo"));

        Assert.Contains("OpenWeather:ApiKey", error.Message);
        Assert.Empty(_stub.RequestUris);
    }
}
