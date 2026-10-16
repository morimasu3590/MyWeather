using MyWeather.Application.HttpClientWrapper;

namespace MyWeather.Application.Tests;

// A /data/2.5/weather response for London, taken from a real API call.
public static class OpenWeatherMapSamples
{
    public static OpenWeatherMapCurrentResponseDto London(params OpenWeatherMapWeatherConditionDto[] weather) => new(
        new OpenWeatherMapCoordDto(-0.1m, 51.5m),
        weather.Length > 0 ? weather : [new OpenWeatherMapWeatherConditionDto(804, "Clouds", "overcast clouds", "04n")],
        new OpenWeatherMapMainDto(14.23, 13.93, 14.23, 14.23, 1022, 85),
        10000,
        new OpenWeatherMapWindDto(3.44, 210, 7.37),
        new OpenWeatherMapCloudsDto(98),
        1792029968,
        new OpenWeatherMapSysDto("GB", 1792045428, 1792084098),
        3600,
        "Lambeth");
}
