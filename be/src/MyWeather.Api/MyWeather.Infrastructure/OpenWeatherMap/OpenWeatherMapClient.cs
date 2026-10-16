using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MyWeather.Application.HttpClientWrapper;
using Polly;

namespace MyWeather.Infrastructure.OpenWeatherMap;

public class OpenWeatherMapClient(HttpClient client, IMemoryCache cache, ILogger<OpenWeatherMapClient> logger)
    : IOpenWeatherMapClient
{
    // City coordinates don't change, so a found geo lookup is reused instead of costing an API call.
    private static readonly TimeSpan GeoCacheDuration = TimeSpan.FromDays(1);

    public async Task<OpenWeatherMapGeoResponseDto?> GetCityGeoTagAsync(string countryCode, string cityName,
        CancellationToken cancellation = default)
    {
        var cacheKey = $"openweathermap:geo:{countryCode.ToUpperInvariant()}:{cityName.ToLowerInvariant()}";
        if (cache.TryGetValue(cacheKey, out OpenWeatherMapGeoResponseDto? cached))
        {
            return cached;
        }

        // The Geo API returns an array of matches ([] when the city is unknown).
        // ReadFromJsonAsync uses web defaults, so "name"/"lat"/"lon" bind case-insensitively.
        var locations = await SendAndReadAsync<List<OpenWeatherMapGeoResponseDto>>(
            $"/geo/1.0/direct?q={Uri.EscapeDataString(cityName)},{Uri.EscapeDataString(countryCode)}&limit=1",
            "geo lookup", countryCode, cityName, cancellation);
        var location = locations?.FirstOrDefault();

        // Unknown cities are not cached, so a later lookup can still find them.
        if (location is not null)
        {
            cache.Set(cacheKey, location, GeoCacheDuration);
        }

        return location;
    }

    public async Task<OpenWeatherMapCurrentResponseDto?> GetCurrentWeatherDataAsync(string countryCode, string cityName
        , CancellationToken cancellation = default)
    {
        var geoResponse = await GetCityGeoTagAsync(countryCode, cityName, cancellation);
        if (geoResponse == null) return null;

        // Invariant culture keeps "51.5" from becoming "51,5" on comma-decimal machines.
        return await SendAndReadAsync<OpenWeatherMapCurrentResponseDto>(
            string.Create(CultureInfo.InvariantCulture,
                $"/data/2.5/weather?lat={geoResponse.Lat}&lon={geoResponse.Lon}&units=metric&lang=en"),
            "current weather", countryCode, cityName, cancellation);
    }

    // Turns every provider-side failure into a WeatherProviderException so callers can degrade gracefully.
    // Caller cancellation and configuration/programming errors are deliberately not caught.
    private async Task<T?> SendAndReadAsync<T>(string url, string operation, string countryCode, string cityName,
        CancellationToken cancellation)
    {
        try
        {
            using var response = await client.GetAsync(url, cancellation);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(cancellation);
        }
        catch (Exception ex) when (IsProviderFailure(ex, cancellation))
        {
            var statusCode = (ex as HttpRequestException)?.StatusCode;
            // The request URL is not logged: the API-key handler has added appid=<key> to it.
            logger.LogWarning(ex, "OpenWeatherMap {Operation} failed for {CountryCode}/{CityName} (status {StatusCode})",
                operation, countryCode, cityName, statusCode?.ToString() ?? "none");
            throw new WeatherProviderException($"OpenWeatherMap {operation} failed.", ex, statusCode);
        }
    }

    private static bool IsProviderFailure(Exception ex, CancellationToken cancellation) => ex switch
    {
        HttpRequestException => true,           // error status (EnsureSuccessStatusCode) or network failure
        JsonException or NotSupportedException => true, // malformed JSON or a non-JSON content type
        ExecutionRejectedException => true,     // resilience pipeline: timeout, open circuit, rate limit
        TaskCanceledException => !cancellation.IsCancellationRequested, // HttpClient timeout, not the caller
        _ => false
    };
}
