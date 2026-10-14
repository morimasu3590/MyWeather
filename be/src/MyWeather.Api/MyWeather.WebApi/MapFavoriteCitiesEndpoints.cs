using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyWeather.Application.FavoriteCity.Queries;
using MyWeather.Application.FavoriteCity.Commands;
namespace MyWeather.WebApi;

public static class MapFavoriteCitiesEndpoints
{
    public static IEndpointRouteBuilder MapFavoriteCitiesEndpointBuilder(this IEndpointRouteBuilder app)
    {
        var groupBuilder = app.MapGroup("/favorites")
            .WithTags("Favorites");

        groupBuilder.MapGet("/{id}:int",
            async (ISender sender, [AsParameters] GetFavoriteCityByIdQueryDto query,
                CancellationToken cancellationToken) =>
            {
                var response = await sender.Send(query, cancellationToken);
                return response != null ? Results.Ok(response) : Results.NotFound();
            }).WithValidationFilter<GetFavoriteCityByIdQueryDto>();

        groupBuilder.MapGet("/", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var response = await sender.Send(new GetAllFavoriteCitiesQueryDto(), cancellationToken);
            return Results.Ok(response);
        });

        groupBuilder.MapPost("/", async (ISender sender, [FromBody] CreateFavoriteCityCommandDto command, CancellationToken cancellationToken) =>
        {
            var response = await sender.Send(command, cancellationToken);
            return Results.Created($"/favorites/{response.Id}", response);
        }).WithValidationFilter<CreateFavoriteCityCommandDto>();
        return app;
    }

    
}