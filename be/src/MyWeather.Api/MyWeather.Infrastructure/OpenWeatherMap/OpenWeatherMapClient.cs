using System.Net.Http.Json;
using MyWeather.Application.HttpClientWrapper;

namespace MyWeather.Infrastructure.OpenWeatherMap;

public class OpenWeatherMapClient(HttpClient client) : IOpenWeatherMapClient
{
    public async Task<OpenWeatherMapGeoResponseDto?> GetCityGeoTagAsync(string countryCode, string cityName,
        CancellationToken cancellation = default)
    {
        string appId = string.Empty;
        // Get Geo tagging
        var response = await client.GetAsync($"/geo/1.0/direct?q={Uri.EscapeDataString(cityName)},{Uri.EscapeDataString(countryCode)}" +
                                             $"&limit=5&appid={appId}", cancellation);
        response.EnsureSuccessStatusCode();

        // The Geo API returns an array of matches ([] when the city is unknown).
        // ReadFromJsonAsync uses web defaults, so "name"/"lat"/"lon" bind case-insensitively.
        var locations = await response.Content.ReadFromJsonAsync<List<OpenWeatherMapGeoResponseDto>>(cancellation);
        return locations?.FirstOrDefault();
    }

    public async Task<OpenWeatherMapCurrentResponseDto?> GetCurrentWeatherDataAsync(string countryCode, string cityName
        , CancellationToken cancellation = default)
    {
        string appId = string.Empty;
        //Get Current
        var geoResponse = await this.GetCityGeoTagAsync(countryCode, cityName, cancellation);
        if (geoResponse == null) return null;
        var response = await client.GetAsync($"/data/4.0/onecall/current" +
                                             $"?lat={geoResponse.Lat}&lon={geoResponse.Lon}&appid={appId}", cancellation);
        
        var currentWeatherData =
            await response.Content.ReadFromJsonAsync<OpenWeatherMapCurrentResponseDto>(cancellation);
        return currentWeatherData;

    }
}