namespace MyWeather.Application.FavoriteCity.Queries;

public record GetAllFavoriteCitiesQueryResponseDto(
    int Id,
    int CityId,
    string CityName,
    string CountryCode,
    string CountryName,
    string Notes
);
