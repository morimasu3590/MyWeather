using MediatR;

namespace MyWeather.Application.Countries.Queries.ByCountryCode;

public class GetCitiesByCountryCodeHandler : IRequestHandler<GetCitiesByCountryCodeDto, IReadOnlyList<GetCitiesByCountryCodeResponseDto>?>
{
    public async Task<IReadOnlyList<GetCitiesByCountryCodeResponseDto>?> Handle(GetCitiesByCountryCodeDto request, CancellationToken cancellationToken)
    {
        // Simulate fetching country data for the given country code
        var response = new GetCitiesByCountryCodeResponseDto(
            CityId: 1,
            CityName: "Sample City",    
            CountryCode: request.CountryCode,
            CountryName: "Sample Country"
        );

        return await Task.FromResult(new List<GetCitiesByCountryCodeResponseDto> { response });
    }
}