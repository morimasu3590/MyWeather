using System.Net;
using System.Net.Http.Json;
using MyWeather.Application.Countries.Queries;
using MyWeather.Application.Countries.Queries.ByCountryCode;
using MyWeather.WebApi.Tests.Infrastructure;

namespace MyWeather.WebApi.Tests.Endpoints;

public class CountriesEndpointsTests : IDisposable
{
    private readonly FakeSenderApiFactory _factory = new();
    private readonly HttpClient _client;

    public CountriesEndpointsTests() => _client = _factory.CreateClient();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetCountries_Returns200WithSenderResponse()
    {
        IReadOnlyList<GetCountriesResponseDto> countries = [new(1, "NO", "Norway"), new(2, "JP", "Japan")];
        _factory.Sender.Returns<GetCountriesQueryDto>(countries);

        var response = await _client.GetAsync("/countries");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(countries, await response.Content.ReadFromJsonAsync<GetCountriesResponseDto[]>());
    }

    [Fact]
    public async Task GetCitiesByCountryCode_Returns200AndSendsCountryCode()
    {
        IReadOnlyList<GetCitiesByCountryCodeResponseDto> cities = [new(10, "Oslo", "NO", "Norway")];
        _factory.Sender.Returns<GetCitiesByCountryCodeDto>(cities);

        var response = await _client.GetAsync("/countries/NO/cities");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(cities, await response.Content.ReadFromJsonAsync<GetCitiesByCountryCodeResponseDto[]>());
        Assert.Equal(new GetCitiesByCountryCodeDto("NO"), Assert.Single(_factory.Sender.Received));
    }

    [Fact]
    public async Task GetCitiesByCountryCode_Returns404WhenCountryNotFound()
    {
        _factory.Sender.Returns<GetCitiesByCountryCodeDto>(null);

        var response = await _client.GetAsync("/countries/ZZ/cities");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("N")]
    [InlineData("NOR")]
    [InlineData("N1")]
    public async Task GetCitiesByCountryCode_Returns400ForInvalidCodeWithoutSending(string countryCode)
    {
        var response = await _client.GetAsync($"/countries/{countryCode}/cities");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_factory.Sender.Received);
    }
}
