using Microsoft.EntityFrameworkCore;

namespace MyWeather.DAL.Tests;

public class SeedDataTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new(keepSeedData: true);

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Seed_ContainsTwentyCountriesIncludingRequiredOnes()
    {
        using var context = _factory.CreateContext();

        var names = await context.Countries.Select(c => c.Name).ToListAsync();

        Assert.Equal(20, names.Count);
        Assert.Contains("Indonesia", names);
        Assert.Contains("Singapore", names);
        Assert.Contains("Malaysia", names);
        Assert.Contains("Australia", names);
    }

    [Fact]
    public async Task Seed_CountryCodesAreUniqueTwoLetterCodes()
    {
        using var context = _factory.CreateContext();

        var codes = await context.Countries.Select(c => c.Code).ToListAsync();

        Assert.Equal(codes.Count, codes.Distinct().Count());
        Assert.All(codes, code => Assert.True(code.Length == 2 && code.All(char.IsAsciiLetterUpper)));
    }

    [Fact]
    public async Task Seed_EveryCountryHasExactlyOneCity()
    {
        using var context = _factory.CreateContext();

        var cityCounts = await context.Countries.Select(c => context.Cities.Count(city => city.CountryId == c.Id)).ToListAsync();

        Assert.All(cityCounts, count => Assert.Equal(1, count));
    }

    [Theory]
    [InlineData("ID", "Jakarta")]
    [InlineData("SG", "Singapore")]
    [InlineData("MY", "Kuala Lumpur")]
    [InlineData("AU", "Canberra")]
    public async Task Seed_CityIsTheCapitalOfItsCountry(string countryCode, string capital)
    {
        using var context = _factory.CreateContext();

        var city = await context.Cities.SingleAsync(c => c.Country.Code == countryCode);

        Assert.Equal(capital, city.Name);
    }
}
