using MyWeather.Application.Tests.Fakes;
using MyWeather.Application.WeatherForecasts.Queries;
using MyWeather.Domain;
using MyWeather.Domain.Model;

namespace MyWeather.Application.Tests;

public class GetWeatherByCityNameHandlerTests
{
    private static Weather NewWeather() => new()
    {
        Id = 5,
        CityId = 10,
        City = new City { Id = 10, Name = "Oslo", Country = new Country { Id = 1, Code = "NO", Name = "Norway" } },
        Date = new DateTime(2026, 3, 4, 12, 30, 45, DateTimeKind.Utc),
        WindSpeed = 5.5,
        WindDirection = 270,
        TemperatureInC = 100,
        DewPoint = 3.2,
        Humidity = 80,
        Pressure = 1013,
        Visibility = 10,
        Cloudiness = EnumCloudiness.PartlyCloudy,
        WeatherDescription = "Partly cloudy"
    };

    [Fact]
    public async Task Handle_MapsWeatherToResponseDto()
    {
        var handler = new GetWeatherByCityNameHandler(new FakeWeatherRepository(NewWeather()));

        var result = await handler.Handle(new GetWeatherByCityNameQueryDto("Oslo"), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(10, result.CityId);
        Assert.Equal("Oslo", result.CityName);
        Assert.Equal("Norway", result.Country);
        Assert.Equal(5.5, result.WindSpeed);
        Assert.Equal(270, result.WindDirection);
        Assert.Equal(10, result.Visibility);
        Assert.Equal(1013, result.Pressure);
        Assert.Equal("PartlyCloudy", result.Cloudiness);
        Assert.Equal(80, result.Humidity);
        Assert.Equal(3.2, result.DewPoint);
        Assert.Equal("Partly cloudy", result.WeatherDescription);
    }

    [Fact]
    public async Task Handle_ReturnsStoredCelsiusAndConvertedFahrenheit()
    {
        var handler = new GetWeatherByCityNameHandler(new FakeWeatherRepository(NewWeather()));

        var result = await handler.Handle(new GetWeatherByCityNameQueryDto("Oslo"), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(100, result.TemperatureC);
        Assert.Equal(212, result.TemperatureF);
    }

    [Fact]
    public async Task Handle_FormatsTimeAsUtcIsoString()
    {
        var handler = new GetWeatherByCityNameHandler(new FakeWeatherRepository(NewWeather()));

        var result = await handler.Handle(new GetWeatherByCityNameQueryDto("Oslo"), CancellationToken.None);

        Assert.Equal("2026-03-04T12:30:45Z", result!.TimeUtc);
    }

    [Fact]
    public async Task Handle_TreatsUnspecifiedKindDateAsUtc()
    {
        var weather = NewWeather();
        weather.Date = new DateTime(2026, 3, 4, 12, 30, 45, DateTimeKind.Unspecified);
        var handler = new GetWeatherByCityNameHandler(new FakeWeatherRepository(weather));

        var result = await handler.Handle(new GetWeatherByCityNameQueryDto("Oslo"), CancellationToken.None);

        Assert.Equal("2026-03-04T12:30:45Z", result!.TimeUtc);
    }

    [Fact]
    public async Task Handle_ConvertsLocalKindDateToUtc()
    {
        var local = new DateTime(2026, 3, 4, 12, 30, 45, DateTimeKind.Local);
        var weather = NewWeather();
        weather.Date = local;
        var handler = new GetWeatherByCityNameHandler(new FakeWeatherRepository(weather));

        var result = await handler.Handle(new GetWeatherByCityNameQueryDto("Oslo"), CancellationToken.None);

        Assert.Equal(local.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture), result!.TimeUtc);
    }

    [Fact]
    public async Task Handle_PassesCityNameAndCancellationTokenToRepository()
    {
        var repository = new FakeWeatherRepository(NewWeather());
        var cts = new CancellationTokenSource();

        await new GetWeatherByCityNameHandler(repository)
            .Handle(new GetWeatherByCityNameQueryDto("Oslo"), cts.Token);

        Assert.Equal("Oslo", repository.ReceivedCityName);
        Assert.Equal(cts.Token, repository.ReceivedCancellationToken);
    }

    [Fact]
    public async Task Handle_ReturnsNullWhenNoWeatherFound()
    {
        var handler = new GetWeatherByCityNameHandler(new FakeWeatherRepository(null));

        var result = await handler.Handle(new GetWeatherByCityNameQueryDto("Atlantis"), CancellationToken.None);

        Assert.Null(result);
    }
}
