using MyWeather.DAL.Repositories;
using MyWeather.Domain.Model;

namespace MyWeather.DAL.Tests;

public class CityRepositoryTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetCitiesByCountryCodeAsync_ReturnsOnlyCitiesOfCountryOrderedByName()
    {
        using (var seed = _factory.CreateContext())
        {
            var norway = new Country { Name = "Norway", Code = "NO" };
            var canada = new Country { Name = "Canada", Code = "CA" };
            seed.Cities.AddRange(
                new City { Name = "Oslo", Country = norway },
                new City { Name = "Bergen", Country = norway },
                new City { Name = "Toronto", Country = canada });
            await seed.SaveChangesAsync();
        }

        using var context = _factory.CreateContext();
        var result = await new CityRepository(context).GetCitiesByCountryCodeAsync("NO");

        Assert.Equal(new[] { "Bergen", "Oslo" }, result.Select(c => c.Name));
    }

    [Fact]
    public async Task GetCitiesByCountryCodeAsync_ReturnsEmptyListForUnknownCountryCode()
    {
        using (var seed = _factory.CreateContext())
        {
            seed.Cities.Add(new City { Name = "Oslo", Country = new Country { Name = "Norway", Code = "NO" } });
            await seed.SaveChangesAsync();
        }

        using var context = _factory.CreateContext();
        var result = await new CityRepository(context).GetCitiesByCountryCodeAsync("XX");

        Assert.Empty(result);
    }
}
