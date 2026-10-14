using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MyWeather.DAL.Tests;

public class MigrationTests : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public MigrationTests() => _connection.Open();

    public void Dispose() => _connection.Dispose();

    private MyWeatherDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<MyWeatherDbContext>().UseSqlite(_connection).Options);

    [Fact]
    public async Task Migrate_CreatesSchemaAndSeedData()
    {
        using var context = CreateContext();

        await context.Database.MigrateAsync();

        Assert.Equal(20, await context.Countries.CountAsync());
        Assert.Equal(20, await context.Cities.CountAsync());
        Assert.Empty(await context.FavoriteCities.ToListAsync());
        Assert.Empty(await context.WeatherRecords.ToListAsync());
    }

    [Fact]
    public void Model_HasNoPendingChangesWithoutMigration()
    {
        using var context = CreateContext();

        // Fails when a model or configuration change was made without `dotnet ef migrations add`.
        Assert.False(context.Database.HasPendingModelChanges());
    }
}
