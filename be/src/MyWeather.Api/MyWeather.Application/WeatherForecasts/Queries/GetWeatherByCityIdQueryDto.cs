using FluentValidation;
using MediatR;

namespace MyWeather.Application.WeatherForecasts.Queries;

public record GetWeatherByCityIdQueryDto(int cityId) : IRequest<GetWeatherByCityIdResponseDto?>;

public class GetWeatherByCityIdQueryDtoValidator : AbstractValidator<GetWeatherByCityIdQueryDto>
{
    public GetWeatherByCityIdQueryDtoValidator()
    {
        RuleFor(x => x.cityId).GreaterThan(0).WithMessage("City ID must be greater than 0.");
    }
}