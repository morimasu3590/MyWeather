using MediatR;
using MyWeather.Domain.Repositories;

namespace MyWeather.Application.Countries.Queries.ByCountryCode;

public class GetCitiesByCountryCodeHandler(ICityRepository cityRepository) : IRequestHandler<GetCitiesByCountryCodeDto, IReadOnlyList<GetCitiesByCountryCodeResponseDto>?>
{
    public async Task<IReadOnlyList<GetCitiesByCountryCodeResponseDto>?> Handle(GetCitiesByCountryCodeDto request, CancellationToken cancellationToken)
    {
        // Simulate fetching country data for the given country code
        var response = await cityRepository.GetCitiesByCountryCodeAsync(request.CountryCode, cancellationToken);
        return response.Count > 0 ?
            response.Select(c => new GetCitiesByCountryCodeResponseDto
            (
                c.Id,
                c.Name,
                c.Country.Code,
                c.Country.Name
            )).ToList() : [];
    }
}