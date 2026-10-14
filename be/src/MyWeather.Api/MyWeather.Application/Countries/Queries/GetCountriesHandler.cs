using MediatR;
using MyWeather.Domain.Repositories;

namespace MyWeather.Application.Countries.Queries;

public class GetCountriesHandler(ICountryRepository countryRepository)
    : IRequestHandler<GetCountriesQueryDto, IReadOnlyList<GetCountriesResponseDto>>
{
    public async Task<IReadOnlyList<GetCountriesResponseDto>> Handle(GetCountriesQueryDto request, CancellationToken cancellationToken)
    {
        // Dummy data until countries come from the database
        var response = await countryRepository.GetAllCountriesAsync(cancellationToken);
        return response.Count > 0 ?
                    [
                        .. response.Select(c => new GetCountriesResponseDto
                        (
                            c.Id,
                            c.Code,
                            c.Name
                        ))
                    ] : [];
    }
}
