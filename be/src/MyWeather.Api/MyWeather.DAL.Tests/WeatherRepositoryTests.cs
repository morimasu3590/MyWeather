using MyWeather.DAL.Repositories;
using MyWeather.Domain;
using MyWeather.Domain.Model;

namespace MyWeather.DAL.Tests;

public class WeatherRepositoryTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    private static Weather NewWeather(City city, DateTime date, EnumCloudiness cloudiness = EnumCloudiness.Clear) => new()
    {
        City = city,
        Date = date,
        Cloudiness = cloudiness,
        TemperatureInC = 10
    };

    [Fact]
    public async Task GetWeatherByCityNameAsync_ReturnsLatestRecordOfCity()
    {
        using (var seed = _factory.CreateContext())
        {
            var norway = new Country { Name = "Norway", Code = "NO" };
            var oslo = new City { Name = "Oslo", Country = norway };
            var bergen = new City { Name = "Bergen", Country = norway };
            seed.WeatherRecords.AddRange(
                NewWeather(oslo, new DateTime(2026, 1, 1)),
                NewWeather(oslo, new DateTime(2026, 1, 3)),
                NewWeather(oslo, new DateTime(2026, 1, 2)),
                NewWeather(bergen, new DateTime(2026, 1, 5)));
            await seed.SaveChangesAsync();
        }

        using var context = _factory.CreateContext();
        var result = await new WeatherRepository(context).GetWeatherByCityNameAsync("Oslo");

        Assert.NotNull(result);
        Assert.Equal(new DateTime(2026, 1, 3), result.Date);
        Assert.Equal("Oslo", result.City.Name);
    }

    [Fact]
    public async Task GetWeatherByCityNameAsync_ReturnsNullForUnknownCity()
    {
        using var context = _factory.CreateContext();

        var result = await new WeatherRepository(context).GetWeatherByCityNameAsync("Atlantis");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetWeatherByCityNameAsync_RoundTripsCloudiness()
    {
        using (var seed = _factory.CreateContext())
        {
            var oslo = new City { Name = "Oslo", Country = new Country { Name = "Norway", Code = "NO" } };
            seed.WeatherRecords.Add(NewWeather(oslo, new DateTime(2026, 1, 1), EnumCloudiness.HeavyRain));
            await seed.SaveChangesAsync();
        }

        using var context = _factory.CreateContext();
        var result = await new WeatherRepository(context).GetWeatherByCityNameAsync("Oslo");

        Assert.Equal(EnumCloudiness.HeavyRain, result!.Cloudiness);
    }
}
