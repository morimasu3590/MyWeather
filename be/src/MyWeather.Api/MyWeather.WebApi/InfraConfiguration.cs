using Microsoft.EntityFrameworkCore;
using MyWeather.DAL;
using FluentValidation;
using MyWeather.Application.WeatherForecasts.Queries;

namespace MyWeather.WebApi;

public static class InfraConfiguration
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Database configuration
        services.AddDbContext<MyWeatherDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
        
    
        // Add other services and configurations as needed
        var applicationAssembly = typeof(GetWeatherByCityIdQueryDto).Assembly;
        services.AddValidatorsFromAssembly(applicationAssembly);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        // services.AddHttpClient<OpenWeatherClient>(client =>
        // {
        //     client.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/");
        //     client.Timeout = TimeSpan.FromSeconds(10);
        // });
    }
}