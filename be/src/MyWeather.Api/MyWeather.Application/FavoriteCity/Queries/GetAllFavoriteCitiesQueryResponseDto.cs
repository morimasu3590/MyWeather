using MyWeather.Application.WeatherForecasts.Queries;
namespace MyWeather.Application.FavoriteCity.Queries;

// A favorite city with its own id and notes, plus its latest stored weather (null when there is none yet).
public record GetAllFavoriteCitiesQueryResponseDto(
    int Id,
    int CityId,
    string CityName,
    string CountryCode,
    string CountryName,
    string Notes,
    GetWeatherByCityResponseDto? Weather
);
