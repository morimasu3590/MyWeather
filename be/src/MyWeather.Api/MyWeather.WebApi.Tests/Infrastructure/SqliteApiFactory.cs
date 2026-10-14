using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyWeather.DAL;
using MyWeather.Domain;
using MyWeather.Domain.Model;

namespace MyWeather.WebApi.Tests.Infrastructure;

// Hosts the full API against a private in-memory SQLite database that holds the model's seed data
// (20 countries, one city each). The database lives only as long as this factory.
public class SqliteApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public SqliteApiFactory()
    {
        _connection.Open();
        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        TestEnvironment.UseIsolatedSettings(builder);
        builder.ConfigureTestServices(services =>
        {
            // EF Core 9+ also keeps the original UseSqlite call as an IDbContextOptionsConfiguration.
            services.RemoveAll<DbContextOptions<MyWeatherDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<MyWeatherDbContext>>();
            services.AddDbContext<MyWeatherDbContext>(options => options.UseSqlite(_connection));
        });
    }

    public MyWeatherDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<MyWeatherDbContext>().UseSqlite(_connection).Options);

    public async Task<City> GetCityAsync(string name)
    {
        await using var context = CreateContext();
        return await context.Cities.AsNoTracking().Include(c => c.Country).SingleAsync(c => c.Name == name);
    }

    public async Task<Weather> AddWeatherAsync(int cityId, DateTime dateUtc, double celsius)
    {
        await using var context = CreateContext();
        var weather = new Weather
        {
            CityId = cityId,
            Date = dateUtc,
            TemperatureInC = celsius,
            WindSpeed = 5.5,
            Humidity = 80,
            Cloudiness = EnumCloudiness.PartlyCloudy,
            WeatherDescription = "Partly cloudy"
        };
        context.WeatherRecords.Add(weather);
        await context.SaveChangesAsync();
        return weather;
    }

    public async Task<FavoriteCity> AddFavoriteAsync(int cityId, string notes = "")
    {
        await using var context = CreateContext();
        var favorite = new FavoriteCity { CityId = cityId, Notes = notes };
        context.FavoriteCities.Add(favorite);
        await context.SaveChangesAsync();
        return favorite;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
