using MyWeather.Application.WeatherForecasts.Queries;

namespace MyWeather.Application.Tests;

public class GetWeatherByCityNameQueryDtoValidatorTests
{
    private readonly GetWeatherByCityNameQueryDtoValidator _validator = new();

    [Theory]
    [InlineData("Oslo")]
    [InlineData("New York")]
    [InlineData("Saint-Denis")]
    [InlineData("St. John's")]
    [InlineData("Zürich")]
    public void Validate_AcceptsValidCityNames(string cityName)
    {
        Assert.True(_validator.Validate(new GetWeatherByCityNameQueryDto(cityName)).IsValid);
    }

    [Theory]
    [InlineData("Oslo1")]
    [InlineData("42")]
    [InlineData("Paris!")]
    [InlineData("a_b")]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsInvalidCityNames(string cityName)
    {
        Assert.False(_validator.Validate(new GetWeatherByCityNameQueryDto(cityName)).IsValid);
    }

    [Fact]
    public void Validate_RejectsNullCityNameWithoutThrowing()
    {
        Assert.False(_validator.Validate(new GetWeatherByCityNameQueryDto(null!)).IsValid);
    }
}
