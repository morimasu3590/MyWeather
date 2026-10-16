using System.Text.Json.Serialization;

namespace MyWeather.Application.HttpClientWrapper;

// Response of the free Current Weather API (GET /data/2.5/weather). The API uses snake_case, so each
// property is mapped explicitly. Values are metric when the request passes units=metric; dt, sunrise
// and sunset are Unix seconds (UTC). Name is the nearest station area, not necessarily the requested city.
public record OpenWeatherMapCurrentResponseDto(
    [property: JsonPropertyName("coord")] OpenWeatherMapCoordDto Coord,
    [property: JsonPropertyName("weather")] IReadOnlyList<OpenWeatherMapWeatherConditionDto> Weather,
    [property: JsonPropertyName("main")] OpenWeatherMapMainDto Main,
    [property: JsonPropertyName("visibility")] int Visibility,
    [property: JsonPropertyName("wind")] OpenWeatherMapWindDto Wind,
    [property: JsonPropertyName("clouds")] OpenWeatherMapCloudsDto Clouds,
    [property: JsonPropertyName("dt")] long Dt,
    [property: JsonPropertyName("sys")] OpenWeatherMapSysDto Sys,
    [property: JsonPropertyName("timezone")] int Timezone,
    [property: JsonPropertyName("name")] string Name);

public record OpenWeatherMapCoordDto(
    [property: JsonPropertyName("lon")] decimal Lon,
    [property: JsonPropertyName("lat")] decimal Lat);

public record OpenWeatherMapMainDto(
    [property: JsonPropertyName("temp")] double Temp,
    [property: JsonPropertyName("feels_like")] double FeelsLike,
    [property: JsonPropertyName("temp_min")] double TempMin,
    [property: JsonPropertyName("temp_max")] double TempMax,
    [property: JsonPropertyName("pressure")] int Pressure,
    [property: JsonPropertyName("humidity")] int Humidity);

public record OpenWeatherMapWindDto(
    [property: JsonPropertyName("speed")] double Speed,
    [property: JsonPropertyName("deg")] int Deg,
    // Often absent in calm conditions.
    [property: JsonPropertyName("gust")] double? Gust);

public record OpenWeatherMapCloudsDto(
    [property: JsonPropertyName("all")] int All);

public record OpenWeatherMapSysDto(
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("sunrise")] long Sunrise,
    [property: JsonPropertyName("sunset")] long Sunset);

public record OpenWeatherMapWeatherConditionDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("main")] string Main,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("icon")] string Icon);
