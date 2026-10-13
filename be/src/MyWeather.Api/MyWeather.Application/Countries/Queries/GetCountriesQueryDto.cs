using MediatR;

namespace MyWeather.Application.Countries.Queries;

public record GetCountriesQueryDto : IRequest<IReadOnlyList<GetCountriesResponseDto>>;
