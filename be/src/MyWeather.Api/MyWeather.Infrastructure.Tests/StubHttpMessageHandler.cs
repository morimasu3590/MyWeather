using System.Net;
using System.Text;

namespace MyWeather.Infrastructure.Tests;

// Terminal handler that records each request and answers with canned responses (or throws a queued
// exception), so no real HTTP is sent.
public class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpResponseMessage>> _responses = new();

    public List<Uri> RequestUris { get; } = [];

    public StubHttpMessageHandler RespondsWith(string json, HttpStatusCode status = HttpStatusCode.OK) =>
        RespondsWith(json, "application/json", status);

    public StubHttpMessageHandler RespondsWith(string body, string mediaType, HttpStatusCode status = HttpStatusCode.OK)
    {
        _responses.Enqueue(() => new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, mediaType)
        });
        return this;
    }

    public StubHttpMessageHandler Throws(Exception exception)
    {
        _responses.Enqueue(() => throw exception);
        return this;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestUris.Add(request.RequestUri!);
        cancellationToken.ThrowIfCancellationRequested();
        var respond = _responses.Count > 0 ? _responses.Dequeue() : () => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        return Task.FromResult(respond());
    }
}
