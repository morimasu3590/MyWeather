using Microsoft.EntityFrameworkCore;

namespace MyWeather.DAL;

public class MyWeatherDbContext : DbContext
{
    public MyWeatherDbContext(DbContextOptions<MyWeatherDbContext> options) : base(options)
    {
        
    }
}
