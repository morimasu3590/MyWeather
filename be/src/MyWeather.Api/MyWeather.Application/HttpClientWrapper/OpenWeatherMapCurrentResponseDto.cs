using System.Text.Json.Serialization;

namespace MyWeather.Application.HttpClientWrapper;

// The API uses snake_case, so each property is mapped explicitly. Temperatures are in Kelvin
// unless the request passes units=metric; dt/sunrise/sunset are Unix seconds (UTC).
public record OpenWeatherMapCurrentResponseDto(
    [property: JsonPropertyName("lat")] decimal Lat,
    [property: JsonPropertyName("lon")] decimal Lon,
    [property: JsonPropertyName("timezone")] string Timezone,
    [property: JsonPropertyName("timezone_offset")] int TimezoneOffset,
    [property: JsonPropertyName("data")] IReadOnlyList<OpenWeatherMapCurrentDataDto> Data);

public record OpenWeatherMapCurrentDataDto(
    [property: JsonPropertyName("dt")] long Dt,
    [property: JsonPropertyName("sunrise")] long Sunrise,
    [property: JsonPropertyName("sunset")] long Sunset,
    [property: JsonPropertyName("temp")] double Temp,
    [property: JsonPropertyName("feels_like")] double FeelsLike,
    [property: JsonPropertyName("pressure")] int Pressure,
    [property: JsonPropertyName("humidity")] int Humidity,
    [property: JsonPropertyName("dew_point")] double DewPoint,
    [property: JsonPropertyName("uvi")] double Uvi,
    [property: JsonPropertyName("clouds")] int Clouds,
    [property: JsonPropertyName("visibility")] int Visibility,
    [property: JsonPropertyName("wind_speed")] double WindSpeed,
    [property: JsonPropertyName("wind_deg")] int WindDeg,
    [property: JsonPropertyName("weather")] IReadOnlyList<OpenWeatherMapWeatherConditionDto> Weather,
    // Absent when there are no active alerts.
    [property: JsonPropertyName("alerts")] IReadOnlyList<OpenWeatherMapAlertDto>? Alerts);

// A national weather alert; start and end are Unix seconds (UTC).
public record OpenWeatherMapAlertDto(
    [property: JsonPropertyName("sender_name")] string SenderName,
    [property: JsonPropertyName("event")] string Event,
    [property: JsonPropertyName("start")] long Start,
    [property: JsonPropertyName("end")] long End,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("tags")] IReadOnlyList<string> Tags);

public record OpenWeatherMapWeatherConditionDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("main")] string Main,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("icon")] string Icon);
