using MediatR;

namespace MyWeather.Application.FavoriteCity.Queries;

public record GetAllFavoriteCitiesQueryDto : IRequest<IReadOnlyList<GetAllFavoriteCitiesQueryResponseDto>>;
