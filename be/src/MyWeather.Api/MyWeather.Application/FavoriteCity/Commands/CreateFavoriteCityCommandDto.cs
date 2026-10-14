using FluentValidation;
using MediatR;
namespace MyWeather.Application.FavoriteCity.Commands;

public record CreateFavoriteCityCommandDto(int CityId, string Notes) : IRequest<CreateFavoriteCityCommandResponseDto>
{
    public class CreateFavoriteCityCommandDtoValidator : AbstractValidator<CreateFavoriteCityCommandDto>
    {
        public CreateFavoriteCityCommandDtoValidator()
        {
            RuleFor(x => x.CityId).GreaterThan(0);
            RuleFor(x => x.Notes).MaximumLength(500);
        }
    }
}