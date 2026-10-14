using MyWeather.Application.Countries.Queries.ByCountryCode;

namespace MyWeather.Application.Tests;

public class GetCitiesByCountryCodeDtoValidatorTests
{
    private readonly GetCitiesByCountryCodeDtoValidator _validator = new();

    [Theory]
    [InlineData("NO")]
    [InlineData("us")]
    public void Validate_AcceptsTwoLetterCodes(string countryCode)
    {
        Assert.True(_validator.Validate(new GetCitiesByCountryCodeDto(countryCode)).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("N")]
    [InlineData("NOR")]
    [InlineData("12")]
    [InlineData("N!")]
    public void Validate_RejectsInvalidCodes(string countryCode)
    {
        Assert.False(_validator.Validate(new GetCitiesByCountryCodeDto(countryCode)).IsValid);
    }
}
