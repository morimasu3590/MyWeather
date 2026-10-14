using MyWeather.Domain.Model;
using MyWeather.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MyWeather.DAL.Repositories;

public class FavoriteCityRepository(MyWeatherDbContext context) : IFavoriteCityRepository
{
    public async Task<FavoriteCity?> GetById(int id, CancellationToken cancellation = default)
    {
        return await context.FavoriteCities.AsNoTracking()
            .Include(fc => fc.City)
            .ThenInclude(c => c.Country)
            .Where(fc => fc.Id == id)
            .FirstOrDefaultAsync(cancellation);
    }

    public async Task<IReadOnlyList<FavoriteCity>> GetAllFavoriteCitiesAsync(CancellationToken cancellationToken = default)
    {
        return await context.FavoriteCities
            .AsNoTracking()
            .Include(fc => fc.City)
            .ThenInclude(c => c.Country)
            .OrderBy(fc => fc.City.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> AddFavoriteCityAsync(FavoriteCity favoriteCity, CancellationToken cancellationToken = default)
    {
        context.FavoriteCities.Add(favoriteCity);
        await context.SaveChangesAsync(cancellationToken);
        return favoriteCity.Id;
    }
}