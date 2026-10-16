using Microsoft.AspNetCore.Hosting;

namespace MyWeather.WebApi.Tests.Infrastructure;

public static class TestEnvironment
{
    // A non-Development environment, so Program.cs skips Database.Migrate() and the OpenAPI endpoints.
    public const string Name = "Testing";

    public const string ApiKey = "test-api-key";

    // Points DefaultConnection at a throwaway file, so a broken DbContext override could never touch
    // the real myweather.db in the repository root. A dummy OpenWeather key satisfies ValidateOnStart;
    // tests never call OpenWeatherMap.
    public static void UseIsolatedSettings(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Name);
        var throwawayDb = Path.Combine(Path.GetTempPath(), $"myweather-tests-{Guid.NewGuid():N}.db");
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={throwawayDb}");
        builder.UseSetting("OpenWeather:ApiKey", ApiKey);
    }
}
