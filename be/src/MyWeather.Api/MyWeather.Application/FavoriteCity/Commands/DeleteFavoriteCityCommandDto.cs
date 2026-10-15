using FluentValidation;
using MediatR;
namespace MyWeather.Application.FavoriteCity.Commands;

// Responds true when the favorite existed and was deleted.
public record DeleteFavoriteCityCommandDto(int Id) : IRequest<bool>
{
    public class DeleteFavoriteCityCommandDtoValidator : AbstractValidator<DeleteFavoriteCityCommandDto>
    {
        public DeleteFavoriteCityCommandDtoValidator()
        {
            // Same message as GetFavoriteCityByIdQueryDtoValidator, so GET and DELETE report a bad id alike.
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Id must be greater than 0.");
        }
    }
}
