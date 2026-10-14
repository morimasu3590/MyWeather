using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MyWeather.DAL;
using MyWeather.DAL.Repositories;
using MyWeather.Domain.Repositories;
using FluentValidation;
using MyWeather.Application.WeatherForecasts.Queries;
using MyWeather.Infrastructure.OpenWeatherMap;

namespace MyWeather.WebApi;

public static class InfraConfiguration
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration, string contentRootPath)
    {
        // Database configuration
        var connectionString = BuildSqliteConnectionString(configuration, contentRootPath);
        services.AddDbContext<MyWeatherDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<IWeatherRepository, WeatherRepository>();
        services.AddScoped<IFavoriteCityRepository, FavoriteCityRepository>();
        
    
        // Add other services and configurations as needed
        var applicationAssembly = typeof(GetWeatherByCityNameQueryDto).Assembly;
        services.AddValidatorsFromAssembly(applicationAssembly);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddHttpClient<IOpenWeatherMapClient, OpenWeatherMapClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.openweathermap.org");
            client.Timeout = TimeSpan.FromSeconds(10);
        }).AddStandardResilienceHandler();
    }

    // A relative DataSource is resolved against the repository root (first parent folder containing
    // .git), so the database file is shared by every launch profile. Without a repository (published
    // or container runs) it falls back to the content root. An absolute DataSource is used as-is.
    private static string BuildSqliteConnectionString(IConfiguration configuration, string contentRootPath)
    {
        var rawConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(rawConnectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
        }

        var builder = new SqliteConnectionStringBuilder(rawConnectionString);
        if (string.IsNullOrWhiteSpace(builder.DataSource) || builder.DataSource == ":memory:")
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' must point to a SQLite database file.");
        }

        if (!Path.IsPathRooted(builder.DataSource))
        {
            builder.DataSource = Path.GetFullPath(
                Path.Combine(FindRepositoryRoot(contentRootPath) ?? contentRootPath, builder.DataSource));
        }

        return builder.ToString();
    }

    private static string? FindRepositoryRoot(string startPath)
    {
        for (var directory = new DirectoryInfo(startPath); directory is not null; directory = directory.Parent)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, ".git")))
            {
                return directory.FullName;
            }
        }

        return null;
    }
}
