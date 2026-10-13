using MediatR;
using FluentValidation;

namespace MyWeather.Application.Countries.Queries.ByCountryCode;

public record GetCitiesByCountryCodeDto(
    string CountryCode
) : IRequest<IReadOnlyList<GetCitiesByCountryCodeResponseDto>?>;
    
public class GetCountriesByCountryCodeDtoValidator : AbstractValidator<GetCitiesByCountryCodeDto>
{
    public GetCountriesByCountryCodeDtoValidator()
    {
        RuleFor(x => x.CountryCode)
            .NotEmpty().WithMessage("Country code is required.")
            .Length(2).WithMessage("Country code must be 2 characters long.");
    }
}
