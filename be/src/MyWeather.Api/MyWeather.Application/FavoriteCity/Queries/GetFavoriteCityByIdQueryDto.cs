using MediatR;
using FluentValidation;


namespace MyWeather.Application.FavoriteCity.Queries;

public record GetFavoriteCityByIdQueryDto(int Id) : IRequest<GetFavoriteCityByIdResponseDto?>;

public class GetFavoriteCityByIdQueryDtoValidator : AbstractValidator<GetFavoriteCityByIdQueryDto>
{
    public GetFavoriteCityByIdQueryDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0.");
    }
}
