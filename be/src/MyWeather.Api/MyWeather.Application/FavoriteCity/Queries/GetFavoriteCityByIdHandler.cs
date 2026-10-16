using MediatR;
using MyWeather.Application.HttpClientWrapper;
using MyWeather.Application.WeatherForecasts.Queries;
using MyWeather.Domain.Repositories;

namespace MyWeather.Application.FavoriteCity.Queries;

public class GetFavoriteCityByIdHandler(
    IFavoriteCityRepository favoriteCityRepository,
    IWeatherRepository weatherRepository,
    IOpenWeatherMapClient openWeatherMapClient) : IRequestHandler<GetFavoriteCityByIdQueryDto, GetFavoriteCityByIdResponseDto?>
{
    public async Task<GetFavoriteCityByIdResponseDto?> Handle(GetFavoriteCityByIdQueryDto request,
        CancellationToken cancellationToken)
    {
        var response = await favoriteCityRepository.GetById(request.Id, cancellationToken);
        if (response != null)
        {
            var weatherFromDb = await weatherRepository.GetWeatherByCityIdAsync(response.CityId, cancellationToken);
            if (weatherFromDb != null)
            {
                return new GetFavoriteCityByIdResponseDto(response.Id, response.Notes,
                    WeatherResponseMapper.ToResponseDto(weatherFromDb));
            }
            else
            {
                OpenWeatherMapCurrentResponseDto? openMapResponseDto;
                try
                {
                    openMapResponseDto = await openWeatherMapClient.GetCurrentWeatherDataAsync(
                        response.City.Country.Code, response.City.Name, cancellationToken);
                }
                catch (WeatherProviderException)
                {
                    // The client has already logged the failure; the favorite is still useful without weather.
                    openMapResponseDto = null;
                }

                return openMapResponseDto != null
                    ? new GetFavoriteCityByIdResponseDto(response.Id, response.Notes,
                        WeatherResponseMapper.ToResponseDto(response.City, response.City.Country, openMapResponseDto))
                    : new GetFavoriteCityByIdResponseDto(response.Id, response.Notes, null);
            }
        }

        return null;
    }
}