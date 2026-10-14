using MyWeather.Application.FavoriteCity.Queries;
using MyWeather.Application.Tests.Fakes;
using MyWeather.Domain.Model;
using FavoriteCityModel = MyWeather.Domain.Model.FavoriteCity;

namespace MyWeather.Application.Tests;

public class GetAllFavoriteCitiesHandlerTests
{
    private static readonly Country Norway = new() { Id = 1, Code = "NO", Name = "Norway" };
    private static readonly Country Japan = new() { Id = 2, Code = "JP", Name = "Japan" };

    [Fact]
    public async Task Handle_MapsFavoritesToResponseDtosInRepositoryOrder()
    {
        var repository = new FakeFavoriteCityRepository([
            new FavoriteCityModel { Id = 1, CityId = 10, Notes = "Fjords", City = new City { Id = 10, Name = "Oslo", Country = Norway } },
            new FavoriteCityModel { Id = 2, CityId = 20, Notes = "", City = new City { Id = 20, Name = "Tokyo", Country = Japan } }
        ]);

        var result = await new GetAllFavoriteCitiesHandler(repository)
            .Handle(new GetAllFavoriteCitiesQueryDto(), CancellationToken.None);

        Assert.Equal(
            [
                new GetAllFavoriteCitiesQueryResponseDto(1, 10, "Oslo", "NO", "Norway", "Fjords"),
                new GetAllFavoriteCitiesQueryResponseDto(2, 20, "Tokyo", "JP", "Japan", "")
            ],
            result);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyListWhenNoFavorites()
    {
        var handler = new GetAllFavoriteCitiesHandler(new FakeFavoriteCityRepository([]));

        var result = await handler.Handle(new GetAllFavoriteCitiesQueryDto(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        var repository = new FakeFavoriteCityRepository([]);
        var cts = new CancellationTokenSource();

        await new GetAllFavoriteCitiesHandler(repository).Handle(new GetAllFavoriteCitiesQueryDto(), cts.Token);

        Assert.Equal(cts.Token, repository.ReceivedCancellationToken);
    }
}
