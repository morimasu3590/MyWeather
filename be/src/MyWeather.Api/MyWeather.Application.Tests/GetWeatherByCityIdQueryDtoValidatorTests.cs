using MyWeather.Application.WeatherForecasts.Queries;

namespace MyWeather.Application.Tests;

public class GetWeatherByCityIdQueryDtoValidatorTests
{
    private readonly GetWeatherByCityIdQueryDtoValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(int.MaxValue)]
    public void Validate_AcceptsPositiveCityIds(int cityId)
    {
        Assert.True(_validator.Validate(new GetWeatherByCityIdQueryDto(cityId)).IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Validate_RejectsNonPositiveCityIds(int cityId)
    {
        var result = _validator.Validate(new GetWeatherByCityIdQueryDto(cityId));

        Assert.False(result.IsValid);
        Assert.Equal("CityId must be greater than 0.", Assert.Single(result.Errors).ErrorMessage);
    }
}
