using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MyWeather.DAL.Tests;

public sealed class TestDbContextFactory : IDisposable
{
    private readonly SqliteConnection _connection;

    public TestDbContextFactory(bool keepSeedData = false)
    {
        // The in-memory database lives only as long as the connection stays open.
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated();

        if (!keepSeedData)
        {
            // The model seeds countries and cities; most tests start from empty tables.
            context.Cities.ExecuteDelete();
            context.Countries.ExecuteDelete();
        }
    }

    public MyWeatherDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MyWeatherDbContext>()
            .UseSqlite(_connection)
            .Options;
        return new MyWeatherDbContext(options);
    }

    public void Dispose() => _connection.Dispose();
}
