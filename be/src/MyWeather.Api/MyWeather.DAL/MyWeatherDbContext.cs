using Microsoft.EntityFrameworkCore;
using MyWeather.Domain.Model;

namespace MyWeather.DAL;

public class MyWeatherDbContext : DbContext
{
    public MyWeatherDbContext(DbContextOptions<MyWeatherDbContext> options) : base(options)
    {
    }

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Weather> WeatherRecords => Set<Weather>();
    public DbSet<FavoriteCity> FavoriteCities => Set<FavoriteCity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyWeatherDbContext).Assembly);
    }
}
