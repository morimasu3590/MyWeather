using System.Net;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using MyWeather.Application.HttpClientWrapper;
using MyWeather.Infrastructure.OpenWeatherMap;
using MyWeather.WebApi.Tests.Infrastructure;

namespace MyWeather.WebApi.Tests.Endpoints;

// Resolves the client from the app's real DI registration, replacing only the network layer,
// to prove the API-key handler and base address are part of the configured pipeline.
public class OpenWeatherMapClientRegistrationTests
{
    private sealed class RecordingHandler : HttpMessageHandler
    {
        public List<Uri> RequestUris { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUris.Add(request.RequestUri!);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""[{ "name": "London", "lat": 51.5, "lon": -0.1 }]""", Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class RecordingFactory : FakeSenderApiFactory
    {
        public RecordingHandler Network { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureTestServices(services =>
                services.AddHttpClient<IOpenWeatherMapClient, OpenWeatherMapClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => Network));
        }
    }

    [Fact]
    public async Task RegisteredClient_SendsRequestsThroughApiKeyHandlerToBaseUrl()
    {
        using var factory = new RecordingFactory();
        using var scope = factory.Services.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IOpenWeatherMapClient>();

        await client.GetCityGeoTagAsync("GB", "London");

        var uri = Assert.Single(factory.Network.RequestUris);
        Assert.Equal("https://api.openweathermap.org/geo/1.0/direct", uri.GetLeftPart(UriPartial.Path));
        Assert.EndsWith($"&appid={TestEnvironment.ApiKey}", uri.Query);
    }
}
