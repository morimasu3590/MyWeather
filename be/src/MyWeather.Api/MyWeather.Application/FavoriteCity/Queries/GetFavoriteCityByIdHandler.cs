using MediatR;
using MyWeather.Application.WeatherForecasts.Queries;
using MyWeather.Domain.Repositories;

namespace MyWeather.Application.FavoriteCity.Queries;

public class GetFavoriteCityByIdHandler(
    IFavoriteCityRepository favoriteCityRepository, 
    IWeatherRepository weatherRepository) : IRequestHandler<GetFavoriteCityByIdQueryDto, GetFavoriteCityByIdResponseDto?>
{
    public async Task<GetFavoriteCityByIdResponseDto?> Handle(GetFavoriteCityByIdQueryDto request,
        CancellationToken cancellationToken)
    {
        var response = await favoriteCityRepository.GetById(request.Id, cancellationToken);
        if (response != null)
        {
            var weather = await weatherRepository.GetWeatherByCityIdAsync(response.CityId,cancellationToken);

            return weather != null
                ? new GetFavoriteCityByIdResponseDto(response.Id, response.Notes,
                    WeatherResponseMapper.ToResponseDto(weather))
                : new GetFavoriteCityByIdResponseDto(response.Id, response.Notes, null);
        }

        return null;
    }
}