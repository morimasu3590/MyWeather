using MyWeather.Application.WeatherForecasts.Queries;
using MyWeather.Domain.Model;

namespace MyWeather.Application.Tests;

public class WeatherResponseMapperTests
{
    private static readonly Country UnitedKingdom = new() { Id = 6, Code = "GB", Name = "United Kingdom" };
    private static readonly City London = new() { Id = 6, Name = "London", Country = UnitedKingdom };

    [Fact]
    public void ToResponseDto_MapsCurrentWeatherApiResponse()
    {
        var result = WeatherResponseMapper.ToResponseDto(London, UnitedKingdom, OpenWeatherMapSamples.London());

        // City and country come from the database, not the API's station name ("Lambeth").
        Assert.Equal((6, "London", "United Kingdom"), (result.CityId, result.CityName, result.Country));
        Assert.Equal("2026-10-15T02:06:08Z", result.TimeUtc); // dt = 1792029968
        Assert.Equal(14.23, result.TemperatureC);
        Assert.Equal(57.614, result.TemperatureF, 6);
        Assert.Equal((3.44, 210.0), (result.WindSpeed, result.WindDirection));
        Assert.Equal((10000.0, 1022.0, 85.0), (result.Visibility, result.Pressure, result.Humidity));
        Assert.Equal(("Clouds", "overcast clouds"), (result.Cloudiness, result.WeatherDescription));
    }

    [Theory]
    [InlineData(14.23, 85, 11.74)]
    [InlineData(20, 100, 20)]   // saturated air: dew point equals temperature
    [InlineData(30, 50, 18.44)]
    [InlineData(10, 0, 10)]     // no humidity reading: falls back to the temperature
    public void ToResponseDto_DerivesDewPointFromTemperatureAndHumidity(double celsius, int humidity, double expected)
    {
        var sample = OpenWeatherMapSamples.London();
        var dto = sample with { Main = sample.Main with { Temp = celsius, Humidity = humidity } };

        var result = WeatherResponseMapper.ToResponseDto(London, UnitedKingdom, dto);

        Assert.Equal(expected, result.DewPoint, 2);
    }

    [Fact]
    public void ToResponseDto_UsesEmptyTextWhenNoWeatherCondition()
    {
        var dto = OpenWeatherMapSamples.London() with { Weather = [] };

        var result = WeatherResponseMapper.ToResponseDto(London, UnitedKingdom, dto);

        Assert.Equal((string.Empty, string.Empty), (result.Cloudiness, result.WeatherDescription));
    }
}
