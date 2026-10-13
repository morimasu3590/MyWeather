namespace MyWeather.Application.Countries.Queries.ByCountryCode;


public record GetCitiesByCountryCodeResponseDto(
    int CityId,
    string CityName, 
    string CountryCode,
    string CountryName
);