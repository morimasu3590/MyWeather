using MediatR;
namespace MyWeather.Application.Countries.Queries;

public class GetCountriesHandler : IRequestHandler<GetCountriesQueryDto, IReadOnlyList<GetCountriesResponseDto>>
{
    public Task<IReadOnlyList<GetCountriesResponseDto>> Handle(GetCountriesQueryDto request, CancellationToken cancellationToken)
    {
        // Dummy data until countries come from the database
        IReadOnlyList<GetCountriesResponseDto> response =
        [
            new(1, "US", "United States"),
            new(2, "CA", "Canada"),
            new(3, "GB", "United Kingdom"),
            new(4, "DE", "Germany"),
            new(5, "JP", "Japan"),
            new(CountryId: 6, CountryCode: "FR", CountryName: "France"),
            new(CountryId: 7, CountryCode: "IT", CountryName: "Italy"),
            new(CountryId: 8, CountryCode: "ES", CountryName: "Spain"),
            new(CountryId: 9, CountryCode: "AU", CountryName: "Australia"),
            new(CountryId: 10, CountryCode: "ID", CountryName: "Indonesia"),
            new(CountryId: 11, CountryCode: "SG", CountryName: "Singapore"),
            new(CountryId: 12, CountryCode: "MY", CountryName: "Malaysia"),
            new(CountryId: 13, CountryCode: "TH", CountryName: "Thailand"),
            new(CountryId: 14, CountryCode: "PH", CountryName: "Philippines"),
            new(CountryId: 15, CountryCode: "VN", CountryName: "Vietnam"),
            new(CountryId: 16, CountryCode: "KR", CountryName: "South Korea"),
            new(CountryId: 17, CountryCode: "CN", CountryName: "China"),
            new(CountryId: 18, CountryCode: "IN", CountryName: "India"),
            new(CountryId: 19, CountryCode: "BR", CountryName: "Brazil"),
            new(CountryId: 20, CountryCode: "MX", CountryName: "Mexico")
        ];

        return Task.FromResult(response);
    }
}
