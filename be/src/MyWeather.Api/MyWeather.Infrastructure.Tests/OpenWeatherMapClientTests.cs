using System.Globalization;
using System.Net;
using Microsoft.Extensions.Caching.Memory;
using MyWeather.Infrastructure.OpenWeatherMap;

namespace MyWeather.Infrastructure.Tests;

public class OpenWeatherMapClientTests
{
    private const string LondonGeoJson = """[{ "name": "London", "lat": 51.5, "lon": -0.1, "country": "GB" }]""";

    private const string CurrentJson = """
        {
          "lat": 51.5, "lon": -0.1, "timezone": "Europe/London", "timezone_offset": 3600,
          "data": [{
            "dt": 1777449371, "sunrise": 1777437375, "sunset": 1777490344,
            "temp": 13.27, "feels_like": 12.17, "pressure": 1024, "humidity": 58, "dew_point": 5.19,
            "uvi": 1.55, "clouds": 0, "visibility": 10000, "wind_speed": 8.23, "wind_deg": 70,
            "weather": [{ "id": 800, "main": "Clear", "description": "sky is clear", "icon": "01d" }]
          }]
        }
        """;

    private readonly StubHttpMessageHandler _stub = new();
    private readonly OpenWeatherMapClient _client;

    public OpenWeatherMapClientTests()
    {
        var http = new HttpClient(new OpenWeatherMapApiKeyHandler("KEY") { InnerHandler = _stub })
        {
            BaseAddress = new Uri("https://api.openweathermap.test")
        };
        _client = new OpenWeatherMapClient(http, new MemoryCache(new MemoryCacheOptions()));
    }

    [Fact]
    public async Task GetCityGeoTag_RequestsGeoApiWithKeyAndMapsFirstMatch()
    {
        _stub.RespondsWith(LondonGeoJson);

        var geo = await _client.GetCityGeoTagAsync("GB", "London");

        Assert.Equal(("London", 51.5m, -0.1m), (geo!.Name, geo.Lat, geo.Lon));
        var uri = Assert.Single(_stub.RequestUris);
        Assert.Equal("/geo/1.0/direct", uri.AbsolutePath);
        Assert.Equal("?q=London,GB&limit=1&appid=KEY", uri.Query);
    }

    [Fact]
    public async Task GetCityGeoTag_ReturnsNullWhenNoMatch()
    {
        _stub.RespondsWith("[]");

        Assert.Null(await _client.GetCityGeoTagAsync("GB", "Atlantis"));
    }

    [Fact]
    public async Task GetCurrentWeatherData_UsesGeoCoordinatesAndMapsSnakeCaseFields()
    {
        _stub.RespondsWith(LondonGeoJson).RespondsWith(CurrentJson);

        var current = await _client.GetCurrentWeatherDataAsync("GB", "London");

        var oneCall = _stub.RequestUris[1];
        Assert.Equal("/data/4.0/onecall/current", oneCall.AbsolutePath);
        Assert.Equal("?lat=51.5&lon=-0.1&units=metric&lang=en&appid=KEY", oneCall.Query);

        Assert.Equal("Europe/London", current!.Timezone);
        var data = Assert.Single(current.Data);
        Assert.Equal((12.17, 5.19, 8.23, 70), (data.FeelsLike, data.DewPoint, data.WindSpeed, data.WindDeg));
        Assert.Equal("sky is clear", Assert.Single(data.Weather).Description);
        Assert.Null(data.Alerts);
    }

    [Fact]
    public async Task GetCurrentWeatherData_FormatsCoordinatesWithInvariantCulture()
    {
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE"); // uses "," as the decimal separator
        try
        {
            _stub.RespondsWith(LondonGeoJson).RespondsWith(CurrentJson);

            await _client.GetCurrentWeatherDataAsync("GB", "London");

            Assert.StartsWith("?lat=51.5&lon=-0.1&", _stub.RequestUris[1].Query);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public async Task GetCurrentWeatherData_ReturnsNullWithoutSecondCallWhenCityUnknown()
    {
        _stub.RespondsWith("[]");

        Assert.Null(await _client.GetCurrentWeatherDataAsync("GB", "Atlantis"));
        Assert.Single(_stub.RequestUris);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task GetCurrentWeatherData_ThrowsWhenOneCallFails(HttpStatusCode status)
    {
        _stub.RespondsWith(LondonGeoJson).RespondsWith("""{ "cod": 401, "message": "Invalid API key" }""", status);

        var error = await Assert.ThrowsAsync<HttpRequestException>(() => _client.GetCurrentWeatherDataAsync("GB", "London"));

        Assert.Equal(status, error.StatusCode);
    }

    [Fact]
    public async Task GetCurrentWeatherData_ReusesCachedGeoLookupForSameCity()
    {
        _stub.RespondsWith(LondonGeoJson).RespondsWith(CurrentJson).RespondsWith(CurrentJson);

        await _client.GetCurrentWeatherDataAsync("GB", "London");
        await _client.GetCurrentWeatherDataAsync("gb", "LONDON");

        Assert.Equal(["/geo/1.0/direct", "/data/4.0/onecall/current", "/data/4.0/onecall/current"],
            _stub.RequestUris.Select(u => u.AbsolutePath));
    }

    [Fact]
    public async Task GetCityGeoTag_DoesNotCacheUnknownCity()
    {
        _stub.RespondsWith("[]").RespondsWith(LondonGeoJson);

        Assert.Null(await _client.GetCityGeoTagAsync("GB", "London"));
        Assert.NotNull(await _client.GetCityGeoTagAsync("GB", "London"));

        Assert.Equal(2, _stub.RequestUris.Count);
    }

    [Fact]
    public async Task GetCurrentWeatherData_MapsAlertObjects()
    {
        const string withAlerts = """
            {
              "lat": 51.5, "lon": -0.1, "timezone": "Europe/London", "timezone_offset": 3600,
              "data": [{
                "dt": 1777449371, "sunrise": 1777437375, "sunset": 1777490344,
                "temp": 13.27, "feels_like": 12.17, "pressure": 1024, "humidity": 58, "dew_point": 5.19,
                "uvi": 1.55, "clouds": 0, "visibility": 10000, "wind_speed": 8.23, "wind_deg": 70,
                "weather": [{ "id": 800, "main": "Clear", "description": "sky is clear", "icon": "01d" }],
                "alerts": [{
                  "sender_name": "Met Office", "event": "Yellow wind warning",
                  "start": 1777449371, "end": 1777490344,
                  "description": "Strong winds expected.", "tags": ["Wind"]
                }]
              }]
            }
            """;
        _stub.RespondsWith(LondonGeoJson).RespondsWith(withAlerts);

        var current = await _client.GetCurrentWeatherDataAsync("GB", "London");

        var alert = Assert.Single(Assert.Single(current!.Data).Alerts!);
        Assert.Equal(("Met Office", "Yellow wind warning", 1777490344L), (alert.SenderName, alert.Event, alert.End));
        Assert.Equal(["Wind"], alert.Tags);
    }
}
