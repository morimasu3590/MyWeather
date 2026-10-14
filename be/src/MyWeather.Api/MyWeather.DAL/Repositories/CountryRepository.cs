using Microsoft.EntityFrameworkCore;
using MyWeather.Domain.Model;
using MyWeather.Domain.Repositories;

namespace MyWeather.DAL.Repositories;

public class CountryRepository : ICountryRepository
{
    private readonly MyWeatherDbContext _context;

    public CountryRepository(MyWeatherDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Country>> GetAllCountriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Countries
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}
