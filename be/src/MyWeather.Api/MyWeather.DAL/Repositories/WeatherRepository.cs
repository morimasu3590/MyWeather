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
