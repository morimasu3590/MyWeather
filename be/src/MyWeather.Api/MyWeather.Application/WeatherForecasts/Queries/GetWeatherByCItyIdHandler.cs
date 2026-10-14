using MediatR;
using MyWeather.Domain.Repositories;

namespace MyWeather.Application.WeatherForecasts.Queries;

public class GetWeatherByCityIdHandler(IWeatherRepository weatherRepository) : IRequestHandler<GetWeatherByCityIdQueryDto, GetWeatherByCityResponseDto?>
{
    public async Task<GetWeatherByCityResponseDto?> Handle(GetWeatherByCityIdQueryDto request, CancellationToken cancellationToken)
    {
        var response = await weatherRepository.GetWeatherByCityIdAsync(request.CityId, cancellationToken);
        return response is null ? null : WeatherResponseMapper.ToResponseDto(response);
    }
}