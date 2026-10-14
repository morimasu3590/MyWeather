using MyWeather.DAL.Repositories;
using MyWeather.Domain.Model;

namespace MyWeather.DAL.Tests;

public class FavoriteCityRepositoryTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetAllFavoriteCitiesAsync_ReturnsFavoritesWithCityAndCountryOrderedByCityName()
    {
        using (var seed = _factory.CreateContext())
        {
            var norway = new Country { Name = "Norway", Code = "NO" };
            var japan = new Country { Name = "Japan", Code = "JP" };
            seed.FavoriteCities.AddRange(
                new FavoriteCity { Notes = "second", City = new City { Name = "Tokyo", Country = japan } },
                new FavoriteCity { Notes = "first", City = new City { Name = "Oslo", Country = norway } });
            await seed.SaveChangesAsync();
        }

        using var context = _factory.CreateContext();
        var result = await new FavoriteCityRepository(context).GetAllFavoriteCitiesAsync();

        Assert.Equal(new[] { "Oslo", "Tokyo" }, result.Select(f => f.City.Name));
        Assert.Equal(new[] { "Norway", "Japan" }, result.Select(f => f.City.Country.Name));
        Assert.Equal(new[] { "first", "second" }, result.Select(f => f.Notes));
    }

    [Fact]
    public async Task GetAllFavoriteCitiesAsync_ReturnsEmptyListWhenNoFavorites()
    {
        using var context = _factory.CreateContext();

        var result = await new FavoriteCityRepository(context).GetAllFavoriteCitiesAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task AddFavoriteCityAsync_PersistsFavoriteAndReturnsGeneratedId()
    {
        int cityId;
        using (var seed = _factory.CreateContext())
        {
            var oslo = new City { Name = "Oslo", Country = new Country { Name = "Norway", Code = "NO" } };
            seed.Cities.Add(oslo);
            await seed.SaveChangesAsync();
            cityId = oslo.Id;
        }

        int id;
        using (var context = _factory.CreateContext())
        {
            id = await new FavoriteCityRepository(context)
                .AddFavoriteCityAsync(new FavoriteCity { CityId = cityId, Notes = "Fjords" });
        }

        Assert.True(id > 0);
        using var verify = _factory.CreateContext();
        var stored = Assert.Single(await new FavoriteCityRepository(verify).GetAllFavoriteCitiesAsync());
        Assert.Equal(id, stored.Id);
        Assert.Equal("Fjords", stored.Notes);
        Assert.Equal("Oslo", stored.City.Name);
    }

    [Fact]
    public async Task GetById_ReturnsFavoriteWithCityAndCountry()
    {
        int tokyoFavoriteId;
        using (var seed = _factory.CreateContext())
        {
            var oslo = new FavoriteCity { Notes = "first", City = new City { Name = "Oslo", Country = new Country { Name = "Norway", Code = "NO" } } };
            var tokyo = new FavoriteCity { Notes = "second", City = new City { Name = "Tokyo", Country = new Country { Name = "Japan", Code = "JP" } } };
            seed.FavoriteCities.AddRange(oslo, tokyo);
            await seed.SaveChangesAsync();
            tokyoFavoriteId = tokyo.Id;
        }

        using var context = _factory.CreateContext();
        var result = await new FavoriteCityRepository(context).GetById(tokyoFavoriteId);

        Assert.NotNull(result);
        Assert.Equal(tokyoFavoriteId, result.Id);
        Assert.Equal("second", result.Notes);
        Assert.Equal("Tokyo", result.City.Name);
        Assert.Equal("Japan", result.City.Country.Name);
    }

    [Fact]
    public async Task GetById_ReturnsNullWhenFavoriteDoesNotExist()
    {
        using var context = _factory.CreateContext();

        Assert.Null(await new FavoriteCityRepository(context).GetById(999));
    }

    [Fact]
    public async Task AddFavoriteCityAsync_ThrowsWhenCityDoesNotExist()
    {
        using var context = _factory.CreateContext();

        await Assert.ThrowsAnyAsync<Exception>(() =>
            new FavoriteCityRepository(context).AddFavoriteCityAsync(new FavoriteCity { CityId = 999 }));
    }
}
