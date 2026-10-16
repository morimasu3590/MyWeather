using Microsoft.EntityFrameworkCore;
using MyWeather.Domain.Model;
using MyWeather.Domain.Repositories;

namespace MyWeather.DAL.Repositories;

public class WeatherRepository : IWeatherRepository
{
    private readonly MyWeatherDbContext _context;

    public WeatherRepository(MyWeatherDbContext context)
    {
        _context = context;
    }

    public async Task<Weather?> GetWeatherByCityIdAsync(int cityId, CancellationToken cancellationToken = default)
    {
        return await _context.WeatherRecords
            .AsNoTracking()
            .Include(w => w.City)
                .ThenInclude(c => c.Country)
            .Where(w => w.City.Id == cityId)
            .OrderByDescending(w => w.Date)
            .ThenByDescending((w => w.Id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Weather>> GetWeatherByCityIdsAsync(IEnumerable<int> cityIds, CancellationToken cancellationToken = default)
    {
        // Materialize once: the sequence may be lazy, and Contains below needs a list EF can translate.
        var ids = cityIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return [];
        }

        return await _context.WeatherRecords
            .AsNoTracking()
            .Include(w => w.City)
                .ThenInclude(c => c.Country)
            .Where(w => ids.Contains(w.CityId))
            .OrderByDescending(w => w.Date)
            .ThenByDescending((w => w.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Weather?> GetWeatherByCityNameAsync(string cityName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cityName))
        {
            return null;
        }

        return await _context.WeatherRecords
            .AsNoTracking()
            .Include(w => w.City)
                .ThenInclude(c => c.Country)
            .Where(w => w.City.Name.ToLower().Equals(cityName.ToLower()))
            .OrderByDescending(w => w.Date)
            .ThenByDescending((w => w.Id))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
