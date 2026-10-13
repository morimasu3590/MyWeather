using Microsoft.EntityFrameworkCore;
using MyWeather.DAL;

namespace MyWeather.WebApi;

public static class InfraConfiguration
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MyWeatherDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
    }
}