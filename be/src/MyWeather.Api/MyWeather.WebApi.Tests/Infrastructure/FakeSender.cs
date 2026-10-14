using MediatR;

namespace MyWeather.WebApi.Tests.Infrastructure;

// Stands in for MediatR so endpoint tests check routing, binding, validation and result mapping only.
public class FakeSender : ISender
{
    private readonly Dictionary<Type, Func<object, object?>> _responses = [];

    public List<object> Received { get; } = [];
    public CancellationToken? ReceivedCancellationToken { get; private set; }

    public FakeSender Returns<TRequest>(object? response) => RespondsWith<TRequest>(_ => response);

    public FakeSender RespondsWith<TRequest>(Func<TRequest, object?> respond)
    {
        _responses[typeof(TRequest)] = request => respond((TRequest)request);
        return this;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        Received.Add(request);
        ReceivedCancellationToken = cancellationToken;
        if (!_responses.TryGetValue(request.GetType(), out var respond))
        {
            throw new InvalidOperationException($"No response configured for {request.GetType().Name}.");
        }

        return Task.FromResult((TResponse)respond(request)!);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest =>
        throw new NotImplementedException();

    public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
