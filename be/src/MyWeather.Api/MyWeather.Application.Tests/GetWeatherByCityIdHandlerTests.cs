using MyWeather.Application.Tests.Fakes;
using MyWeather.Application.WeatherForecasts.Queries;
using MyWeather.Domain;
using MyWeather.Domain.Model;

namespace MyWeather.Application.Tests;

public class GetWeatherByCityIdHandlerTests
{
    private static readonly Weather OsloWeather = new()
    {
        Id = 1,
        CityId = 10,
        City = new City { Id = 10, Name = "Oslo", Country = new Country { Id = 1, Code = "NO", Name = "Norway" } },
        Date = new DateTime(2026, 3, 4, 12, 0, 0, DateTimeKind.Utc),
        TemperatureInC = 100,
        WindSpeed = 5.5,
        Cloudiness = EnumCloudiness.PartlyCloudy,
        WeatherDescription = "Partly cloudy"
    };

    [Fact]
    public async Task Handle_MapsWeatherOfCity()
    {
        var handler = new GetWeatherByCityIdHandler(new FakeWeatherRepository(OsloWeather));

        var result = await handler.Handle(new GetWeatherByCityIdQueryDto(10), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal((10, "Oslo", "Norway"), (result.CityId, result.CityName, result.Country));
        Assert.Equal(100, result.TemperatureC);
        Assert.Equal(212, result.TemperatureF);
    }

    [Fact]
    public async Task Handle_ReturnsNullWhenNoWeather()
    {
        var handler = new GetWeatherByCityIdHandler(new FakeWeatherRepository(null));

        var result = await handler.Handle(new GetWeatherByCityIdQueryDto(10), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_PassesCityIdAndCancellationTokenToRepository()
    {
        var repository = new FakeWeatherRepository(OsloWeather);
        var cts = new CancellationTokenSource();

        await new GetWeatherByCityIdHandler(repository).Handle(new GetWeatherByCityIdQueryDto(10), cts.Token);

        Assert.Equal(10, repository.ReceivedCityId);
        Assert.Equal(cts.Token, repository.ReceivedCancellationToken);
    }
}
