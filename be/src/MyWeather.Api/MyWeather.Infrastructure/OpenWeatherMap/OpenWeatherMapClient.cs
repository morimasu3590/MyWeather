using System.Globalization;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using MyWeather.Application.HttpClientWrapper;

namespace MyWeather.Infrastructure.OpenWeatherMap;

public class OpenWeatherMapClient(HttpClient client, IMemoryCache cache) : IOpenWeatherMapClient
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

        // Get Geo tagging
        using var response = await client.GetAsync($"/geo/1.0/direct?q={Uri.EscapeDataString(cityName)},{Uri.EscapeDataString(countryCode)}" +
                                                   $"&limit=1", cancellation);
        response.EnsureSuccessStatusCode();

        // The Geo API returns an array of matches ([] when the city is unknown).
        // ReadFromJsonAsync uses web defaults, so "name"/"lat"/"lon" bind case-insensitively.
        var locations = await response.Content.ReadFromJsonAsync<List<OpenWeatherMapGeoResponseDto>>(cancellation);
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

        //Get Current
        var geoResponse = await this.GetCityGeoTagAsync(countryCode, cityName, cancellation);
        if (geoResponse == null) return null;
        // Invariant culture keeps "51.5" from becoming "51,5" on comma-decimal machines.
        using var response = await client.GetAsync(string.Create(CultureInfo.InvariantCulture,
            $"/data/4.0/onecall/current?lat={geoResponse.Lat}&lon={geoResponse.Lon}&units=metric&lang=en"), cancellation);
        response.EnsureSuccessStatusCode();

        var currentWeatherData =
            await response.Content.ReadFromJsonAsync<OpenWeatherMapCurrentResponseDto>(cancellation);
        return currentWeatherData;

    }
}
