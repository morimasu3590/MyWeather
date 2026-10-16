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

    [Fact]
    public async Task GetWeatherByCityNameAsync_ReadsDatesBackAsUtc()
    {
        var local = new DateTime(2026, 5, 6, 7, 8, 9, DateTimeKind.Local);
        using (var seed = _factory.CreateContext())
        {
            var oslo = new City { Name = "Oslo", Country = new Country { Name = "Norway", Code = "NO" } };
            var bergen = new City { Name = "Bergen", Country = oslo.Country };
            seed.WeatherRecords.AddRange(
                NewWeather(oslo, local),
                NewWeather(bergen, new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Unspecified)));
            await seed.SaveChangesAsync();
        }

        using var context = _factory.CreateContext();
        var repository = new WeatherRepository(context);
        var fromLocal = await repository.GetWeatherByCityNameAsync("Oslo");
        var fromUnspecified = await repository.GetWeatherByCityNameAsync("Bergen");

        Assert.Equal(DateTimeKind.Utc, fromLocal!.Date.Kind);
        Assert.Equal(local.ToUniversalTime(), fromLocal.Date);
        Assert.Equal(DateTimeKind.Utc, fromUnspecified!.Date.Kind);
        Assert.Equal(new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc), fromUnspecified.Date);
    }

    private async Task<(int OsloId, int BergenId, int TokyoId)> SeedThreeCitiesAsync()
    {
        using var seed = _factory.CreateContext();
        var norway = new Country { Name = "Norway", Code = "NO" };
        var oslo = new City { Name = "Oslo", Country = norway };
        var bergen = new City { Name = "Bergen", Country = norway };
        var tokyo = new City { Name = "Tokyo", Country = new Country { Name = "Japan", Code = "JP" } };
        seed.WeatherRecords.AddRange(
            NewWeather(oslo, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            NewWeather(bergen, new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)),
            NewWeather(tokyo, new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc)));
        await seed.SaveChangesAsync();
        return (oslo.Id, bergen.Id, tokyo.Id);
    }

    [Fact]
    public async Task GetWeatherByCityIdsAsync_ReturnsOnlyRecordsOfRequestedCitiesNewestFirst()
    {
        var (osloId, bergenId, _) = await SeedThreeCitiesAsync();

        using var context = _factory.CreateContext();
        var result = await new WeatherRepository(context).GetWeatherByCityIdsAsync([osloId, bergenId]);

        Assert.Equal(new[] { "Bergen", "Oslo" }, result.Select(w => w.City.Name));
        Assert.All(result, w => Assert.NotNull(w.City.Country));
    }

    [Fact]
    public async Task GetWeatherByCityIdsAsync_IgnoresDuplicateIds()
    {
        var (osloId, _, _) = await SeedThreeCitiesAsync();

        using var context = _factory.CreateContext();
        var result = await new WeatherRepository(context).GetWeatherByCityIdsAsync([osloId, osloId]);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetWeatherByCityIdsAsync_AcceptsLazySequence()
    {
        var (osloId, _, tokyoId) = await SeedThreeCitiesAsync();
        IEnumerable<int> Lazy() { yield return osloId; yield return tokyoId; }

        using var context = _factory.CreateContext();
        var result = await new WeatherRepository(context).GetWeatherByCityIdsAsync(Lazy());

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetWeatherByCityIdsAsync_ReturnsEmptyListForNoIds()
    {
        using var context = _factory.CreateContext();

        var result = await new WeatherRepository(context).GetWeatherByCityIdsAsync([]);

        Assert.Empty(result);
    }
}
