using MyWeather.Domain.Model;

namespace MyWeather.DAL.Seed;

// One city per country: its capital. A city's Id and CountryId match the country's Id in CountrySeed.
public static class CitySeed
{
    public static IReadOnlyList<City> Data { get; } =
    [
        new() { Id = 1, Name = "Jakarta", CountryId = 1 },
        new() { Id = 2, Name = "Singapore", CountryId = 2 },
        new() { Id = 3, Name = "Kuala Lumpur", CountryId = 3 },
        new() { Id = 4, Name = "Canberra", CountryId = 4 },
        new() { Id = 5, Name = "Washington, D.C.", CountryId = 5 },
        new() { Id = 6, Name = "London", CountryId = 6 },
        new() { Id = 7, Name = "Ottawa", CountryId = 7 },
        new() { Id = 8, Name = "Tokyo", CountryId = 8 },
        new() { Id = 9, Name = "Berlin", CountryId = 9 },
        new() { Id = 10, Name = "Paris", CountryId = 10 },
        new() { Id = 11, Name = "New Delhi", CountryId = 11 },
        new() { Id = 12, Name = "Beijing", CountryId = 12 },
        new() { Id = 13, Name = "Brasília", CountryId = 13 },
        new() { Id = 14, Name = "Oslo", CountryId = 14 },
        new() { Id = 15, Name = "Wellington", CountryId = 15 },
        new() { Id = 16, Name = "Bangkok", CountryId = 16 },
        new() { Id = 17, Name = "Seoul", CountryId = 17 },
        new() { Id = 18, Name = "Rome", CountryId = 18 },
        new() { Id = 19, Name = "Madrid", CountryId = 19 },
        new() { Id = 20, Name = "Cairo", CountryId = 20 },
    ];
}
