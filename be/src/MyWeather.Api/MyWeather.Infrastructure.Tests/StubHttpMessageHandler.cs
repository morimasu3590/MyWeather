using System.Net;
using System.Text;

namespace MyWeather.Infrastructure.Tests;

// Terminal handler that records each request and answers with canned JSON, so no real HTTP is sent.
public class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<(HttpStatusCode Status, string Json)> _responses = new();

    public List<Uri> RequestUris { get; } = [];

    public StubHttpMessageHandler RespondsWith(string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        _responses.Enqueue((status, json));
        return this;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestUris.Add(request.RequestUri!);
        var (status, json) = _responses.Count > 0 ? _responses.Dequeue() : (HttpStatusCode.OK, "{}");
        return Task.FromResult(new HttpResponseMessage(status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
    }
}
