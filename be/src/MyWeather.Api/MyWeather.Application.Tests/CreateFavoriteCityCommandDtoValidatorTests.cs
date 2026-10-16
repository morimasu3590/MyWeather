using MyWeather.Application.FavoriteCity.Commands;

namespace MyWeather.Application.Tests;

public class CreateFavoriteCityCommandDtoValidatorTests
{
    private readonly CreateFavoriteCityCommandDto.CreateFavoriteCityCommandDtoValidator _validator = new();

    [Fact]
    public void Validate_AcceptsPositiveCityIdAndShortNotes()
    {
        Assert.True(_validator.Validate(new CreateFavoriteCityCommandDto(1, "Nice place")).IsValid);
    }

    [Fact]
    public void Validate_AcceptsNotesOfExactlyMaxLength()
    {
        Assert.True(_validator.Validate(new CreateFavoriteCityCommandDto(1, new string('a', 500))).IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_RejectsNonPositiveCityId(int cityId)
    {
        Assert.False(_validator.Validate(new CreateFavoriteCityCommandDto(cityId, "")).IsValid);
    }

    [Fact]
    public void Validate_RejectsNotesLongerThanMaxLength()
    {
        Assert.False(_validator.Validate(new CreateFavoriteCityCommandDto(1, new string('a', 501))).IsValid);
    }
}
