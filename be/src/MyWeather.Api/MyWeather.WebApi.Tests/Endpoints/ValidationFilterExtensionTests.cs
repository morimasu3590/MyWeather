using System.Net;
using System.Net.Http.Json;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace MyWeather.WebApi.Tests.Endpoints;

public class ValidationFilterExtensionTests : IAsyncDisposable
{
    public record ItemRequest(int Value);

    public record UnvalidatedRequest(int Value);

    private class ItemRequestValidator : AbstractValidator<ItemRequest>
    {
        public ItemRequestValidator() => RuleFor(x => x.Value).GreaterThan(0).WithMessage("Value must be positive.");
    }

    private record ValidationError(string PropertyName, string ErrorMessage);

    private readonly WebApplication _app;
    private readonly HttpClient _client;
    private int _handlerCalls;

    public ValidationFilterExtensionTests()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddScoped<IValidator<ItemRequest>, ItemRequestValidator>();
        _app = builder.Build();

        _app.MapGet("/items/{value:int}", ([AsParameters] ItemRequest request) =>
        {
            _handlerCalls++;
            return Results.Ok(request);
        }).WithValidationFilter<ItemRequest>();

        _app.MapGet("/unvalidated/{value:int}", ([AsParameters] UnvalidatedRequest request) =>
        {
            _handlerCalls++;
            return Results.Ok(request);
        }).WithValidationFilter<UnvalidatedRequest>();

        // The filter looks for an ItemRequest argument, but this handler has none.
        _app.MapGet("/raw/{value:int}", (int value) =>
        {
            _handlerCalls++;
            return Results.Ok(value);
        }).WithValidationFilter<ItemRequest>();

        _app.StartAsync().GetAwaiter().GetResult();
        _client = _app.GetTestClient();
    }

    public async ValueTask DisposeAsync() => await _app.DisposeAsync();

    [Fact]
    public async Task ValidRequest_ReachesHandler()
    {
        var response = await _client.GetAsync("/items/5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, _handlerCalls);
    }

    [Fact]
    public async Task InvalidRequest_Returns400WithErrorsAndSkipsHandler()
    {
        var response = await _client.GetAsync("/items/0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = Assert.Single((await response.Content.ReadFromJsonAsync<ValidationError[]>())!);
        Assert.Equal(new ValidationError("Value", "Value must be positive."), error);
        Assert.Equal(0, _handlerCalls);
    }

    [Fact]
    public async Task RequestWithoutRegisteredValidator_PassesThrough()
    {
        var response = await _client.GetAsync("/unvalidated/0");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, _handlerCalls);
    }

    [Fact]
    public async Task HandlerWithoutRequestArgument_PassesThrough()
    {
        var response = await _client.GetAsync("/raw/0");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, _handlerCalls);
    }
}
