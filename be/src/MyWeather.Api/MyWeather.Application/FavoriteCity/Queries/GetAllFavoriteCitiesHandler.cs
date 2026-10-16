using MediatR;
using MyWeather.Domain.Repositories;
using MyWeather.Application.WeatherForecasts.Queries;

namespace MyWeather.Application.FavoriteCity.Queries;

public class GetAllFavoriteCitiesHandler(IFavoriteCityRepository favoriteCityRepository, IWeatherRepository weatherRepository)
    : IRequestHandler<GetAllFavoriteCitiesQueryDto, IReadOnlyList<GetAllFavoriteCitiesQueryResponseDto>>
{
    public async Task<IReadOnlyList<GetAllFavoriteCitiesQueryResponseDto>> Handle(GetAllFavoriteCitiesQueryDto request, CancellationToken cancellationToken)
    {
        var favoriteCities = await favoriteCityRepository.GetAllFavoriteCitiesAsync(cancellationToken);
        if (favoriteCities.Count == 0)
        {
            return [];
        }

        // One query for all favorites: the repositories share a scoped DbContext, so one query per
        // city with Task.WhenAll would not be safe.
        var weathers = await weatherRepository.GetWeatherByCityIdsAsync(favoriteCities.Select(fc => fc.CityId), cancellationToken);
        var latestByCityId = weathers
            .GroupBy(w => w.CityId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(w => w.Date).ThenByDescending(w => w.Id).First());

        // Every favorite is listed; one without a weather record yet has null weather.
        return
        [
            .. favoriteCities.Select(fc => new GetAllFavoriteCitiesQueryResponseDto(
                fc.Id,
                fc.CityId,
                fc.City.Name,
                fc.City.Country.Code,
                fc.City.Country.Name,
                fc.Notes,
                latestByCityId.TryGetValue(fc.CityId, out var weather) ? WeatherResponseMapper.ToResponseDto(weather) : null))
        ];
    }
}
