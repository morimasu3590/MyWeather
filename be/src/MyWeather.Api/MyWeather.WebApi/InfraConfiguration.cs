using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MyWeather.DAL;
using MyWeather.DAL.Repositories;
using MyWeather.Domain.Repositories;
using FluentValidation;
using Microsoft.Extensions.Options;
using MyWeather.Application.WeatherForecasts.Queries;
using MyWeather.Infrastructure.OpenWeatherMap;
using MyWeather.Application.HttpClientWrapper;
using MyWeather.WebApi.Options;

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
        // Fail at startup, not on the first weather request, when the key or base URL is missing.
        services.AddOptions<OpenWeatherOptions>()
            .Bind(configuration.GetSection(OpenWeatherOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "OpenWeather:ApiKey is required.")
            // The scheme check matters: on macOS/Linux "/some/path" parses as an absolute file:// URI.
            .Validate(o => Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out var uri) && uri.Scheme is "https" or "http",
                "OpenWeather:BaseUrl must be an absolute URL.")
            .ValidateOnStart();
        services.AddMemoryCache();
        // No HttpClient.Timeout: it would cap all retries together. The standard resilience handler
        // applies its own per-attempt and total timeouts.
        services.AddHttpClient<IOpenWeatherMapClient, OpenWeatherMapClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<OpenWeatherOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        })
        // Outside the resilience handler, so the key is added once and retries resend the same request.
        .AddHttpMessageHandler(sp => new OpenWeatherMapApiKeyHandler(
            sp.GetRequiredService<IOptions<OpenWeatherOptions>>().Value.ApiKey))
        .AddStandardResilienceHandler();
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
