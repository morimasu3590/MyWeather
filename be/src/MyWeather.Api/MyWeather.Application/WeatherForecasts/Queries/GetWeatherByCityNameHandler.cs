using MediatR;
using MyWeather.Domain.Repositories;

namespace MyWeather.Application.WeatherForecasts.Queries;

public class GetWeatherByCityNameHandler(IWeatherRepository weatherRepository) : IRequestHandler<GetWeatherByCityNameQueryDto, GetWeatherByCityResponseDto?>
{
    public async Task<GetWeatherByCityResponseDto?> Handle(GetWeatherByCityNameQueryDto request, CancellationToken cancellationToken)
    {
        var response = await weatherRepository.GetWeatherByCityNameAsync(request.CityName, cancellationToken);
        return response is null ? null : WeatherResponseMapper.ToResponseDto(response);
    }
}
