using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MyWeather.Application.HttpClientWrapper;
using MyWeather.Infrastructure.OpenWeatherMap;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace MyWeather.Infrastructure.Tests;

public class OpenWeatherMapClientFailureTests
{
    private const string LondonGeoJson = """[{ "name": "London", "lat": 51.5, "lon": -0.1, "country": "GB" }]""";
    private const string ApiKey = "SECRET-KEY";

    private readonly StubHttpMessageHandler _stub = new();
    private readonly CapturingLogger<OpenWeatherMapClient> _logger = new();
    private readonly OpenWeatherMapClient _client;

    public OpenWeatherMapClientFailureTests()
    {
        var http = new HttpClient(new OpenWeatherMapApiKeyHandler(ApiKey) { InnerHandler = _stub })
        {
            BaseAddress = new Uri("https://api.openweathermap.test")
        };
        _client = new OpenWeatherMapClient(http, new MemoryCache(new MemoryCacheOptions()), _logger);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task WeatherCallErrorStatus_ThrowsWeatherProviderExceptionWithStatus(HttpStatusCode status)
    {
        _stub.RespondsWith(LondonGeoJson).RespondsWith("""{ "cod": 401, "message": "Invalid API key" }""", status);

        var error = await Assert.ThrowsAsync<WeatherProviderException>(() => _client.GetCurrentWeatherDataAsync("GB", "London"));

        Assert.Equal(status, error.StatusCode);
        Assert.IsType<HttpRequestException>(error.InnerException);
    }

    [Fact]
    public async Task GeoCallErrorStatus_ThrowsAndIsNotCached()
    {
        _stub.RespondsWith("oops", "text/plain", HttpStatusCode.InternalServerError).RespondsWith(LondonGeoJson);

        var error = await Assert.ThrowsAsync<WeatherProviderException>(() => _client.GetCityGeoTagAsync("GB", "London"));
        Assert.Equal(HttpStatusCode.InternalServerError, error.StatusCode);

        // The failure was not cached: the next lookup asks the Geo API again and succeeds.
        Assert.NotNull(await _client.GetCityGeoTagAsync("GB", "London"));
        Assert.Equal(2, _stub.RequestUris.Count);
    }

    public static TheoryData<Exception> TransportFailures => new()
    {
        new HttpRequestException("Connection refused"),
        new TimeoutRejectedException("Resilience timeout"),
        new BrokenCircuitException("Circuit open"),
        new TaskCanceledException("HttpClient timeout"),
    };

    [Theory]
    [MemberData(nameof(TransportFailures))]
    public async Task TransportFailure_ThrowsWeatherProviderExceptionWithoutStatus(Exception failure)
    {
        _stub.RespondsWith(LondonGeoJson).Throws(failure);

        var error = await Assert.ThrowsAsync<WeatherProviderException>(() => _client.GetCurrentWeatherDataAsync("GB", "London"));

        Assert.Same(failure, error.InnerException);
        Assert.Null(error.StatusCode);
    }

    [Fact]
    public async Task MalformedJson_ThrowsWeatherProviderException()
    {
        _stub.RespondsWith(LondonGeoJson).RespondsWith("""{ "main": { "temp": "not a number" } }""");

        var error = await Assert.ThrowsAsync<WeatherProviderException>(() => _client.GetCurrentWeatherDataAsync("GB", "London"));

        Assert.IsType<JsonException>(error.InnerException);
    }

    [Fact]
    public async Task NonJsonBody_ThrowsWeatherProviderException()
    {
        _stub.RespondsWith(LondonGeoJson).RespondsWith("<html>Bad gateway</html>", "text/html");

        var error = await Assert.ThrowsAsync<WeatherProviderException>(() => _client.GetCurrentWeatherDataAsync("GB", "London"));

        // .NET 10 reports it as JsonException; older runtimes rejected the content type with NotSupportedException.
        Assert.True(error.InnerException is JsonException or NotSupportedException, error.InnerException?.GetType().Name);
    }

    [Fact]
    public async Task CallerCancellation_IsNotWrapped()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _client.GetCurrentWeatherDataAsync("GB", "London", cts.Token));
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task Failure_IsLoggedAsWarningWithoutTheApiKey()
    {
        _stub.RespondsWith(LondonGeoJson).RespondsWith("{}", HttpStatusCode.Unauthorized);

        await Assert.ThrowsAsync<WeatherProviderException>(() => _client.GetCurrentWeatherDataAsync("GB", "London"));

        var (level, message, exception) = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Warning, level);
        Assert.Equal("OpenWeatherMap current weather failed for GB/London (status Unauthorized)", message);
        Assert.DoesNotContain(ApiKey, message);
        Assert.DoesNotContain(ApiKey, exception!.ToString());
    }
}
