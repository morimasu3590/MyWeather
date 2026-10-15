using MyWeather.Application.FavoriteCity.Commands;

namespace MyWeather.Application.Tests;

public class DeleteFavoriteCityCommandDtoValidatorTests
{
    private readonly DeleteFavoriteCityCommandDto.DeleteFavoriteCityCommandDtoValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(int.MaxValue)]
    public void Validate_AcceptsPositiveIds(int id)
    {
        Assert.True(_validator.Validate(new DeleteFavoriteCityCommandDto(id)).IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Validate_RejectsNonPositiveIds(int id)
    {
        var result = _validator.Validate(new DeleteFavoriteCityCommandDto(id));

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal(("Id", "Id must be greater than 0."), (error.PropertyName, error.ErrorMessage));
    }
}
