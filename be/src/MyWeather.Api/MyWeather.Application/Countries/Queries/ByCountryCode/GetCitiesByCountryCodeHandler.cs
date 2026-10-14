using MediatR;
using MyWeather.Domain.Repositories;

namespace MyWeather.Application.Countries.Queries.ByCountryCode;

public class GetCitiesByCountryCodeHandler(ICountryRepository countryRepository, ICityRepository cityRepository)
    : IRequestHandler<GetCitiesByCountryCodeDto, IReadOnlyList<GetCitiesByCountryCodeResponseDto>?>
{
    public async Task<IReadOnlyList<GetCitiesByCountryCodeResponseDto>?> Handle(GetCitiesByCountryCodeDto request, CancellationToken cancellationToken)
    {
        var country = await countryRepository.GetCountryByCodeAsync(request.CountryCode, cancellationToken);
        if (country is null)
        {
            return null;
        }

        var cities = await cityRepository.GetCitiesByCountryCodeAsync(request.CountryCode, cancellationToken);
        return
        [
            .. cities.Select(c => new GetCitiesByCountryCodeResponseDto
            (
                c.Id,
                c.Name,
                c.Country.Code,
                c.Country.Name
            ))
        ];
    }
}
