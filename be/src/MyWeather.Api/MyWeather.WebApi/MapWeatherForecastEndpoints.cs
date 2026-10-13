using MediatR;
using MyWeather.Application.WeatherForecasts.Queries;

namespace MyWeather.WebApi;

public static class MapWeatherForecastEndpoints
{

    public static IEndpointRouteBuilder MapWeatherForecastEndpointBuilder(this IEndpointRouteBuilder app)
    {

        var groupBuilder = app.MapGroup("/weathers")
            .WithTags("Weathers");

        groupBuilder.MapGet("/{cityName}", async (ISender sender, [AsParameters] GetWeatherByCityNameQueryDto query, CancellationToken cancellationToken) =>
                await GetWeatherForecastByCityNameAsync(sender, query, cancellationToken)
        ).WithValidationFilter<GetWeatherByCityNameQueryDto>();

        return app;
    }

    private static async Task<IResult> GetWeatherForecastByCityNameAsync(ISender sender, GetWeatherByCityNameQueryDto query, CancellationToken cancellationToken)
    {
        var response = await sender.Send(query, cancellationToken);

        return response is not null ? Results.Ok(response) : Results.NotFound();
    }
}