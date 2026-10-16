using MediatR;
using FluentValidation;

namespace MyWeather.Application.WeatherForecasts.Queries;

public record GetWeatherByCityIdQueryDto(int CityId) : IRequest<GetWeatherByCityResponseDto?>;

public class GetWeatherByCityIdQueryDtoValidator : AbstractValidator<GetWeatherByCityIdQueryDto>
{
    public GetWeatherByCityIdQueryDtoValidator()
    {
        RuleFor(x => x.CityId)
            .GreaterThan(0)
            .WithMessage("CityId must be greater than 0.");
    }
}
