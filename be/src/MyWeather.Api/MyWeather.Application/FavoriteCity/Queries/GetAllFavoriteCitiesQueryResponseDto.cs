using MyWeather.Application.WeatherForecasts.Queries;
namespace MyWeather.Application.FavoriteCity.Queries;

// The weather of a favorite city, plus the favorite's own id and notes.
public record GetAllFavoriteCitiesQueryResponseDto(
    int Id,
    string Notes,
    GetWeatherByCityResponseDto Weather
);

