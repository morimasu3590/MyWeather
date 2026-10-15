using System.Net;

namespace MyWeather.Application.HttpClientWrapper;

// The weather provider could not be used: unreachable, timed out, rejected the request (e.g. 401, 429, 5xx)
// or returned an unreadable response. Callers can degrade gracefully instead of failing the whole request.
public sealed class WeatherProviderException(string message, Exception innerException, HttpStatusCode? statusCode = null)
    : Exception(message, innerException)
{
    // The provider's HTTP status when it answered with an error; null for network, timeout or parse failures.
    public HttpStatusCode? StatusCode { get; } = statusCode;
}
