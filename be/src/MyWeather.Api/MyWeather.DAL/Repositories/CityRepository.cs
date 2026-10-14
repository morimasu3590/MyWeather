using Microsoft.EntityFrameworkCore;
using MyWeather.Domain.Model;
using MyWeather.Domain.Repositories;

namespace MyWeather.DAL.Repositories;

public class CityRepository : ICityRepository
{
    private readonly MyWeatherDbContext _context;

    public CityRepository(MyWeatherDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<City>> GetCitiesByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return [];
        }

        return await _context.Cities
            .AsNoTracking()
            .Where(c => c.Country.Code.ToLower().Equals(countryCode.ToLower()))
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}
