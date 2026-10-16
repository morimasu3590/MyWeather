namespace MyWeather.Infrastructure.OpenWeatherMap;

// Adds the OpenWeatherMap API key (appid query parameter) to every request sent by the typed client,
// so OpenWeatherMapClient builds URLs without knowing the key.
public sealed class OpenWeatherMapApiKeyHandler(string apiKey) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OpenWeather:ApiKey is not configured.");
        }

        if (request.RequestUri is not null)
        {
            var uri = new UriBuilder(request.RequestUri);
            var query = uri.Query.TrimStart('?');
            var appId = $"appid={Uri.EscapeDataString(apiKey)}";
            uri.Query = query.Length == 0 ? appId : $"{query}&{appId}";
            request.RequestUri = uri.Uri;
        }

        return base.SendAsync(request, cancellationToken);
    }
}
