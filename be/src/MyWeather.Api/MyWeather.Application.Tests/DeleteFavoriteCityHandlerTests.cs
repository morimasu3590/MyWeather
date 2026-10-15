using MyWeather.Application.FavoriteCity.Commands;
using MyWeather.Application.Tests.Fakes;
using FavoriteCityModel = MyWeather.Domain.Model.FavoriteCity;

namespace MyWeather.Application.Tests;

public class DeleteFavoriteCityHandlerTests
{
    private readonly FakeFavoriteCityRepository _repository = new([new FavoriteCityModel { Id = 3, CityId = 10, Notes = "Fjords" }]);

    [Fact]
    public async Task Handle_ReturnsTrueAndDeletesTheGivenId()
    {
        var cts = new CancellationTokenSource();

        var deleted = await new DeleteFavoriteCityHandler(_repository).Handle(new DeleteFavoriteCityCommandDto(3), cts.Token);

        Assert.True(deleted);
        Assert.Equal([3], _repository.DeletedIds);
        Assert.Equal(cts.Token, _repository.ReceivedCancellationToken);
    }

    [Fact]
    public async Task Handle_ReturnsFalseWhenDeletingTheSameIdAgain()
    {
        var handler = new DeleteFavoriteCityHandler(_repository);

        Assert.True(await handler.Handle(new DeleteFavoriteCityCommandDto(3), CancellationToken.None));
        Assert.False(await handler.Handle(new DeleteFavoriteCityCommandDto(3), CancellationToken.None));
        Assert.Null(await _repository.GetById(3));
    }

    [Fact]
    public async Task Handle_ReturnsFalseWhenFavoriteDoesNotExist()
    {
        var deleted = await new DeleteFavoriteCityHandler(_repository).Handle(new DeleteFavoriteCityCommandDto(99), CancellationToken.None);

        Assert.False(deleted);
    }
}
