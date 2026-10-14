using FluentValidation;
using MediatR;

namespace MyWeather.Application.WeatherForecasts.Queries;

public record GetWeatherByCityNameQueryDto(string CityName) : IRequest<GetWeatherByCityNameResponseDto?>;

public class GetWeatherByCityNameQueryDtoValidator : AbstractValidator<GetWeatherByCityNameQueryDto>
{
    public GetWeatherByCityNameQueryDtoValidator()
    {
        RuleFor(x => x.CityName).Must(NotContainNumbersOrSpaces).WithMessage("City name cannot contain numbers or special characters.");
    }

    private bool NotContainNumbersOrSpaces(string cityName)
    {
        return !cityName.Any(char.IsDigit) && !string.IsNullOrWhiteSpace(cityName);
    }
}