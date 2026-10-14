using MyWeather.Application.Countries.Queries.ByCountryCode;
using MyWeather.Application.Tests.Fakes;
using MyWeather.Domain.Model;

namespace MyWeather.Application.Tests;

public class GetCitiesByCountryCodeHandlerTests
{
    private static readonly Country Norway = new() { Id = 1, Code = "NO", Name = "Norway" };

    [Fact]
    public async Task Handle_MapsCitiesToResponseDtos()
    {
        var repository = new FakeCityRepository([
            new City { Id = 10, Name = "Bergen", Country = Norway },
            new City { Id = 11, Name = "Oslo", Country = Norway }
        ]);

        var result = await new GetCitiesByCountryCodeHandler(repository)
            .Handle(new GetCitiesByCountryCodeDto("NO"), CancellationToken.None);

        Assert.Equal(
            [
                new GetCitiesByCountryCodeResponseDto(10, "Bergen", "NO", "Norway"),
                new GetCitiesByCountryCodeResponseDto(11, "Oslo", "NO", "Norway")
            ],
            result);
    }

    [Fact]
    public async Task Handle_PassesCountryCodeAndCancellationTokenToRepository()
    {
        var repository = new FakeCityRepository([]);
        var cts = new CancellationTokenSource();

        await new GetCitiesByCountryCodeHandler(repository)
            .Handle(new GetCitiesByCountryCodeDto("NO"), cts.Token);

        Assert.Equal("NO", repository.ReceivedCountryCode);
        Assert.Equal(cts.Token, repository.ReceivedCancellationToken);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyListWhenNoCities()
    {
        var handler = new GetCitiesByCountryCodeHandler(new FakeCityRepository([]));

        var result = await handler.Handle(new GetCitiesByCountryCodeDto("NO"), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
