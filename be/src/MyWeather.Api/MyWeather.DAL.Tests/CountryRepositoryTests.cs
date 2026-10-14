using MyWeather.DAL.Repositories;
using MyWeather.Domain.Model;

namespace MyWeather.DAL.Tests;

public class CountryRepositoryTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetAllCountriesAsync_ReturnsAllCountriesOrderedByName()
    {
        using (var seed = _factory.CreateContext())
        {
            seed.Countries.AddRange(
                new Country { Name = "Norway", Code = "NO" },
                new Country { Name = "Canada", Code = "CA" },
                new Country { Name = "Egypt", Code = "EG" });
            await seed.SaveChangesAsync();
        }

        using var context = _factory.CreateContext();
        var result = await new CountryRepository(context).GetAllCountriesAsync();

        Assert.Equal(new[] { "Canada", "Egypt", "Norway" }, result.Select(c => c.Name));
    }

    [Fact]
    public async Task GetAllCountriesAsync_ReturnsEmptyListWhenNoCountries()
    {
        using var context = _factory.CreateContext();

        var result = await new CountryRepository(context).GetAllCountriesAsync();

        Assert.Empty(result);
    }
}
