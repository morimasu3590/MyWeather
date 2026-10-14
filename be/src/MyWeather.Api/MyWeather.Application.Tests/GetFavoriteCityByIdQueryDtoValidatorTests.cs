using MyWeather.Application.FavoriteCity.Queries;

namespace MyWeather.Application.Tests;

public class GetFavoriteCityByIdQueryDtoValidatorTests
{
    private readonly GetFavoriteCityByIdQueryDtoValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(int.MaxValue)]
    public void Validate_AcceptsPositiveIds(int id)
    {
        Assert.True(_validator.Validate(new GetFavoriteCityByIdQueryDto(id)).IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Validate_RejectsNonPositiveIds(int id)
    {
        var result = _validator.Validate(new GetFavoriteCityByIdQueryDto(id));

        Assert.False(result.IsValid);
        Assert.Equal("Id must be greater than 0.", Assert.Single(result.Errors).ErrorMessage);
    }
}
