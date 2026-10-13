using MediatR;
using MyWeather.Application.WeatherForecasts.Queries;

namespace MyWeather.WebApi;

public static class MapWeatherForecastEndpoints
{

    public static IEndpointRouteBuilder MapWeatherForecastEndpointBuilder(this IEndpointRouteBuilder app)
    {

        var groupBuilder = app.MapGroup("/weathers")
            .WithTags("Weathers");

        groupBuilder.MapGet("/{cityId:int}", async (ISender sender, [AsParameters] GetWeatherByCityIdQueryDto query, CancellationToken cancellationToken) =>
                await GetWeatherForecastByCityIdAsync(sender, query, cancellationToken)
        ).WithValidationFilter<GetWeatherByCityIdQueryDto>();

        return app;
    }

    private static async Task<IResult> GetWeatherForecastByCityIdAsync(ISender sender, GetWeatherByCityIdQueryDto query, CancellationToken cancellationToken)
    {
        var response = await sender.Send(query, cancellationToken);

        return response is not null ? Results.Ok(response) : Results.NotFound();
    }
}