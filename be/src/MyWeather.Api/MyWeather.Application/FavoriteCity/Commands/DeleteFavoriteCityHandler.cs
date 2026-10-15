using MediatR;
using MyWeather.Domain.Repositories;

namespace MyWeather.Application.FavoriteCity.Commands;

public class DeleteFavoriteCityHandler(IFavoriteCityRepository favoriteCityRepository)
    : IRequestHandler<DeleteFavoriteCityCommandDto, bool>
{
    public Task<bool> Handle(DeleteFavoriteCityCommandDto request, CancellationToken cancellationToken) =>
        favoriteCityRepository.DeleteFavoriteCityAsync(request.Id, cancellationToken);
}
