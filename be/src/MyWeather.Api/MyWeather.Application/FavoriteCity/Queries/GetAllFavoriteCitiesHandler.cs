using MediatR;
using MyWeather.Domain.Repositories;

namespace MyWeather.Application.FavoriteCity.Queries;

public class GetAllFavoriteCitiesHandler(IFavoriteCityRepository favoriteCityRepository)
    : IRequestHandler<GetAllFavoriteCitiesQueryDto, IReadOnlyList<GetAllFavoriteCitiesQueryResponseDto>>
{
    public async Task<IReadOnlyList<GetAllFavoriteCitiesQueryResponseDto>> Handle(GetAllFavoriteCitiesQueryDto request, CancellationToken cancellationToken)
    {
        var favoriteCities = await favoriteCityRepository.GetAllFavoriteCitiesAsync(cancellationToken);
        return
        [
            .. favoriteCities.Select(fc => new GetAllFavoriteCitiesQueryResponseDto
            (
                fc.Id,
                fc.CityId,
                fc.City.Name,
                fc.City.Country.Code,
                fc.City.Country.Name,
                fc.Notes
            ))
        ];
    }
}
