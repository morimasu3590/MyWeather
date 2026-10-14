using System.Net;
using System.Net.Http.Json;
using MyWeather.Application.Countries.Queries;
using MyWeather.Application.Countries.Queries.ByCountryCode;
using MyWeather.WebApi.Tests.Infrastructure;

namespace MyWeather.WebApi.Tests.Integration;

// Read-only against the seed data, so the whole class shares one database.
public class CountriesIntegrationTests(SqliteApiFactory factory) : IClassFixture<SqliteApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetCountries_ReturnsAllSeededCountries()
    {
        var countries = await _client.GetFromJsonAsync<GetCountriesResponseDto[]>("/countries");

        Assert.NotNull(countries);
        Assert.Equal(20, countries.Length);
        Assert.Contains(countries, c => c is { CountryCode: "ID", CountryName: "Indonesia" });
    }

    [Fact]
    public async Task GetCitiesByCountryCode_ReturnsTheCountrysCity()
    {
        var cities = await _client.GetFromJsonAsync<GetCitiesByCountryCodeResponseDto[]>("/countries/JP/cities");

        var tokyo = Assert.Single(cities!);
        Assert.Equal(("Tokyo", "JP", "Japan"), (tokyo.CityName, tokyo.CountryCode, tokyo.CountryName));
    }

    [Fact]
    public async Task GetCitiesByCountryCode_Returns404ForUnknownCountry()
    {
        var response = await _client.GetAsync("/countries/ZZ/cities");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCitiesByCountryCode_Returns400ForInvalidCode()
    {
        var response = await _client.GetAsync("/countries/J1/cities");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
