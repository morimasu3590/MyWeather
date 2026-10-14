using FluentValidation;
using MediatR;

namespace MyWeather.Application.WeatherForecasts.Queries;

public record GetWeatherByCityNameQueryDto(string CityName) : IRequest<GetWeatherByCityNameResponseDto?>;

public class GetWeatherByCityNameQueryDtoValidator : AbstractValidator<GetWeatherByCityNameQueryDto>
{
    public GetWeatherByCityNameQueryDtoValidator()
    {
        RuleFor(x => x.CityName)
            .Must(BeValidCityName)
            .WithMessage("City name can only contain letters, spaces, hyphens, apostrophes and periods.");
    }

    private static bool BeValidCityName(string? cityName)
    {
        return !string.IsNullOrWhiteSpace(cityName) && cityName.All(IsAllowedCharacter);
    }

    private static bool IsAllowedCharacter(char c) => char.IsLetter(c) || c is ' ' or '-' or '\'' or '.';
}
