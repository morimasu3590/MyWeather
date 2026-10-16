using MediatR;
using MyWeather.Application.Countries.Queries;
using MyWeather.Application.Countries.Queries.ByCountryCode;

namespace MyWeather.WebApi;

public static class MapCountriesEndpoints
{
    public static IEndpointRouteBuilder MapCountriesEndpointBuilder(this IEndpointRouteBuilder app)
    {
        var groupBuilder = app.MapGroup("/countries")
            .WithTags("Countries");

        groupBuilder.MapGet("/", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var query = new GetCountriesQueryDto();
            var response = await sender.Send(query, cancellationToken);
            return Results.Ok(response);
        });

        groupBuilder.MapGet("/{countryCode}/cities", async (ISender sender, [AsParameters] GetCitiesByCountryCodeDto query, CancellationToken cancellationToken) =>
        {
            var response = await sender.Send(query, cancellationToken);
            return response is not null ? Results.Ok(response) : Results.NotFound();
        }).WithValidationFilter<GetCitiesByCountryCodeDto>();

        return app;
    }
}