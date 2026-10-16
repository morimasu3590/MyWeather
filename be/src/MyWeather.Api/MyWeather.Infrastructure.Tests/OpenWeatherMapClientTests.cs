using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using MyWeather.Infrastructure.OpenWeatherMap;

namespace MyWeather.Infrastructure.Tests;

public class OpenWeatherMapClientTests
{
    private const string LondonGeoJson = """[{ "name": "London", "lat": 51.5, "lon": -0.1, "country": "GB" }]""";

    // A real /data/2.5/weather response for London.
    private const string CurrentJson = """
        {
          "coord": { "lon": -0.1, "lat": 51.5 },
          "weather": [{ "id": 804, "main": "Clouds", "description": "overcast clouds", "icon": "04n" }],
          "base": "stations",
          "main": { "temp": 14.23, "feels_like": 13.93, "temp_min": 14.23, "temp_max": 14.23,
                    "pressure": 1022, "humidity": 85, "sea_level": 1022, "grnd_level": 1019 },
          "visibility": 10000,
          "wind": { "speed": 3.44, "deg": 210, "gust": 7.37 },
          "clouds": { "all": 98 },
          "dt": 1792029968,
          "sys": { "country": "GB", "sunrise": 1792045428, "sunset": 1792084098 },
          "timezone": 3600, "id": 6545250, "name": "Lambeth", "cod": 200
        }
        """;

    private const string CurrentWeatherPath = "/data/2.5/weather";

    private readonly StubHttpMessageHandler _stub = new();
    private readonly OpenWeatherMapClient _client;

    public OpenWeatherMapClientTests()
    {
        var http = new HttpClient(new OpenWeatherMapApiKeyHandler("KEY") { InnerHandler = _stub })
        {
            BaseAddress = new Uri("https://api.openweathermap.test")
        };
        _client = new OpenWeatherMapClient(http, new MemoryCache(new MemoryCacheOptions()), NullLogger<OpenWeatherMapClient>.Instance);
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

        var weatherCall = _stub.RequestUris[1];
        Assert.Equal(CurrentWeatherPath, weatherCall.AbsolutePath);
        Assert.Equal("?lat=51.5&lon=-0.1&units=metric&lang=en&appid=KEY", weatherCall.Query);

        Assert.NotNull(current);
        Assert.Equal((14.23, 13.93, 1022, 85), (current.Main.Temp, current.Main.FeelsLike, current.Main.Pressure, current.Main.Humidity));
        Assert.Equal((3.44, 210, (double?)7.37), (current.Wind.Speed, current.Wind.Deg, current.Wind.Gust));
        Assert.Equal((1792029968L, 1792045428L, 3600), (current.Dt, current.Sys.Sunrise, current.Timezone));
        Assert.Equal((98, 10000), (current.Clouds.All, current.Visibility));
        Assert.Equal("overcast clouds", Assert.Single(current.Weather).Description);
    }

    [Fact]
    public async Task GetCurrentWeatherData_AllowsMissingWindGust()
    {
        _stub.RespondsWith(LondonGeoJson).RespondsWith(CurrentJson.Replace(""", "gust": 7.37""", ""));

        var current = await _client.GetCurrentWeatherDataAsync("GB", "London");

        Assert.Null(current!.Wind.Gust);
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

    [Fact]
    public async Task GetCurrentWeatherData_ReusesCachedGeoLookupForSameCity()
    {
        _stub.RespondsWith(LondonGeoJson).RespondsWith(CurrentJson).RespondsWith(CurrentJson);

        await _client.GetCurrentWeatherDataAsync("GB", "London");
        await _client.GetCurrentWeatherDataAsync("gb", "LONDON");

        Assert.Equal(["/geo/1.0/direct", CurrentWeatherPath, CurrentWeatherPath],
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
}
