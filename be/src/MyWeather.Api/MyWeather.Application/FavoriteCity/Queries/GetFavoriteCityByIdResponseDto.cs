using MyWeather.Application.WeatherForecasts.Queries;

namespace MyWeather.Application.FavoriteCity.Queries;

public record GetFavoriteCityByIdResponseDto(int Id,
    string Notes,
    GetWeatherByCityResponseDto? Weather);
