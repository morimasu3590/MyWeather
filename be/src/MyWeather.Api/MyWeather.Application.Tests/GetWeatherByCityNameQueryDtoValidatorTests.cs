using MyWeather.Application.WeatherForecasts.Queries;

namespace MyWeather.Application.Tests;

public class GetWeatherByCityNameQueryDtoValidatorTests
{
    private readonly GetWeatherByCityNameQueryDtoValidator _validator = new();

    [Theory]
    [InlineData("Oslo")]
    [InlineData("New York")]
    public void Validate_AcceptsCityNamesWithoutDigits(string cityName)
    {
        Assert.True(_validator.Validate(new GetWeatherByCityNameQueryDto(cityName)).IsValid);
    }

    [Theory]
    [InlineData("Oslo1")]
    [InlineData("42")]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsCityNamesWithDigitsOrBlank(string cityName)
    {
        Assert.False(_validator.Validate(new GetWeatherByCityNameQueryDto(cityName)).IsValid);
    }
}
