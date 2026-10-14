using MediatR;
using MyWeather.Domain.Repositories;


namespace MyWeather.Application.FavoriteCity.Commands;

public class CreateFavoriteCityHandler(IFavoriteCityRepository favoriteCityRepository, ICityRepository cityRepository) : IRequestHandler<CreateFavoriteCityCommandDto, CreateFavoriteCityCommandResponseDto>
{
    public async Task<CreateFavoriteCityCommandResponseDto> Handle(CreateFavoriteCityCommandDto request, CancellationToken cancellationToken)
    {
        var city = await cityRepository.GetCityByIdAsync(request.CityId, cancellationToken);
        if (city is null)
        {
            throw new ArgumentException($"City with ID {request.CityId} does not exist.");
        }

        var existingFavorites = await favoriteCityRepository.GetAllFavoriteCitiesAsync(cancellationToken);
        if (existingFavorites.Any(fc => fc.CityId == request.CityId))
        {
            throw new ArgumentException($"City with ID {request.CityId} is already a favorite.");
        }

        if (existingFavorites.Count >= 3)
        {
            throw new InvalidOperationException("Cannot add more than 3 favorite cities.");
        }   

        var favoriteCity = new Domain.Model.FavoriteCity
        {
            CityId = request.CityId,
            Notes = request.Notes
        };

        var id = await favoriteCityRepository.AddFavoriteCityAsync(favoriteCity, cancellationToken);

        return new CreateFavoriteCityCommandResponseDto(id);
    }
}   
