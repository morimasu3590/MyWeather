using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MyWeather.DAL;
using FluentValidation;
using MyWeather.Application.WeatherForecasts.Queries;

namespace MyWeather.WebApi;

public static class InfraConfiguration
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration, string contentRootPath)
    {
        // Database configuration
        // The SQLite file lives in the repo root, four levels above the WebApi project folder.
        var connectionString = new SqliteConnectionStringBuilder(configuration.GetConnectionString("DefaultConnection"));
        connectionString.DataSource = Path.GetFullPath(
            Path.Combine(contentRootPath, "..", "..", "..", "..", connectionString.DataSource));
        services.AddDbContext<MyWeatherDbContext>(options =>
            options.UseSqlite(connectionString.ToString()));
        
    
        // Add other services and configurations as needed
        var applicationAssembly = typeof(GetWeatherByCityNameQueryDto).Assembly;
        services.AddValidatorsFromAssembly(applicationAssembly);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        // services.AddHttpClient<OpenWeatherClient>(client =>
        // {
        //     client.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/");
        //     client.Timeout = TimeSpan.FromSeconds(10);
        // });
    }
}