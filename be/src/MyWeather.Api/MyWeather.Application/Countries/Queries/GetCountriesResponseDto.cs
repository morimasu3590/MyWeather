namespace MyWeather.Application.Countries.Queries;

public record GetCountriesResponseDto(
    int CountryId,
    string CountryCode,
    string CountryName
);