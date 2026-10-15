using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using MyWeather.WebApi.Tests.Infrastructure;

namespace MyWeather.WebApi.Tests.Endpoints;

public class OpenWeatherOptionsValidationTests
{
    // Applies the usual isolated settings, then overrides one OpenWeather setting.
    private sealed class OverridingFactory(string key, string value) : FakeSenderApiFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseSetting(key, value);
        }
    }

    [Fact]
    public void ValidSettings_AppStarts()
    {
        using var factory = new FakeSenderApiFactory();

        factory.CreateClient();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void MissingApiKey_FailsAtStartup(string apiKey)
    {
        using var factory = new OverridingFactory("OpenWeather:ApiKey", apiKey);

        var error = Assert.Throws<OptionsValidationException>(() => factory.CreateClient());

        Assert.Contains("OpenWeather:ApiKey is required.", error.Failures);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not a url")]
    [InlineData("/relative/path")]
    public void InvalidBaseUrl_FailsAtStartup(string baseUrl)
    {
        using var factory = new OverridingFactory("OpenWeather:BaseUrl", baseUrl);

        var error = Assert.Throws<OptionsValidationException>(() => factory.CreateClient());

        Assert.Contains("OpenWeather:BaseUrl must be an absolute URL.", error.Failures);
    }
}
