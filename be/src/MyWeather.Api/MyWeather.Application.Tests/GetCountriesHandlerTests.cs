using MyWeather.Application.Countries.Queries;
using MyWeather.Application.Tests.Fakes;
using MyWeather.Domain.Model;

namespace MyWeather.Application.Tests;

public class GetCountriesHandlerTests
{
    [Fact]
    public async Task Handle_MapsCountriesToResponseDtosInRepositoryOrder()
    {
        var repository = new FakeCountryRepository([
            new Country { Id = 1, Code = "CA", Name = "Canada" },
            new Country { Id = 2, Code = "NO", Name = "Norway" }
        ]);
        var cts = new CancellationTokenSource();

        var result = await new GetCountriesHandler(repository).Handle(new GetCountriesQueryDto(), cts.Token);

        Assert.Equal(
            [new GetCountriesResponseDto(1, "CA", "Canada"), new GetCountriesResponseDto(2, "NO", "Norway")],
            result);
        Assert.Equal(cts.Token, repository.ReceivedCancellationToken);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyListWhenNoCountries()
    {
        var handler = new GetCountriesHandler(new FakeCountryRepository([]));

        var result = await handler.Handle(new GetCountriesQueryDto(), CancellationToken.None);

        Assert.Empty(result);
    }
}
