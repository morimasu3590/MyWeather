using MyWeather.Application.Countries.Queries.ByCountryCode;
using MyWeather.Application.Tests.Fakes;
using MyWeather.Domain.Model;

namespace MyWeather.Application.Tests;

public class GetCitiesByCountryCodeHandlerTests
{
    private static readonly Country Norway = new() { Id = 1, Code = "NO", Name = "Norway" };

    private static GetCitiesByCountryCodeHandler CreateHandler(
        IReadOnlyList<Country> countries, IReadOnlyList<City> cities, out FakeCityRepository cityRepository)
    {
        cityRepository = new FakeCityRepository(cities);
        return new GetCitiesByCountryCodeHandler(new FakeCountryRepository(countries), cityRepository);
    }

    [Fact]
    public async Task Handle_MapsCitiesToResponseDtos()
    {
        var handler = CreateHandler(
            [Norway],
            [
                new City { Id = 10, Name = "Bergen", Country = Norway },
                new City { Id = 11, Name = "Oslo", Country = Norway }
            ],
            out _);

        var result = await handler.Handle(new GetCitiesByCountryCodeDto("NO"), CancellationToken.None);

        Assert.Equal(
            [
                new GetCitiesByCountryCodeResponseDto(10, "Bergen", "NO", "Norway"),
                new GetCitiesByCountryCodeResponseDto(11, "Oslo", "NO", "Norway")
            ],
            result);
    }

    [Fact]
    public async Task Handle_PassesCountryCodeAndCancellationTokenToCityRepository()
    {
        var handler = CreateHandler([Norway], [], out var cityRepository);
        var cts = new CancellationTokenSource();

        await handler.Handle(new GetCitiesByCountryCodeDto("NO"), cts.Token);

        Assert.Equal("NO", cityRepository.ReceivedCountryCode);
        Assert.Equal(cts.Token, cityRepository.ReceivedCancellationToken);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyListWhenCountryExistsWithoutCities()
    {
        var handler = CreateHandler([Norway], [], out _);

        var result = await handler.Handle(new GetCitiesByCountryCodeDto("NO"), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ReturnsNullWhenCountryDoesNotExist()
    {
        var handler = CreateHandler([Norway], [], out var cityRepository);

        var result = await handler.Handle(new GetCitiesByCountryCodeDto("ZZ"), CancellationToken.None);

        Assert.Null(result);
        Assert.Null(cityRepository.ReceivedCountryCode);
    }
}
