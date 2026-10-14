using MediatR;
using MyWeather.Domain.Repositories;

namespace MyWeather.Application.Countries.Queries;

public class GetCountriesHandler(ICountryRepository countryRepository)
    : IRequestHandler<GetCountriesQueryDto, IReadOnlyList<GetCountriesResponseDto>>
{
    public async Task<IReadOnlyList<GetCountriesResponseDto>> Handle(GetCountriesQueryDto request, CancellationToken cancellationToken)
    {
        var countries = await countryRepository.GetAllCountriesAsync(cancellationToken);
        return
        [
            .. countries.Select(c => new GetCountriesResponseDto
            (
                c.Id,
                c.Code,
                c.Name
            ))
        ];
    }
}
