using MyWeather.Domain.Model;

namespace MyWeather.DAL.Seed;

public static class CountrySeed
{
    public static IReadOnlyList<Country> Data { get; } =
    [
        new() { Id = 1, Code = "ID", Name = "Indonesia" },
        new() { Id = 2, Code = "SG", Name = "Singapore" },
        new() { Id = 3, Code = "MY", Name = "Malaysia" },
        new() { Id = 4, Code = "AU", Name = "Australia" },
        new() { Id = 5, Code = "US", Name = "United States" },
        new() { Id = 6, Code = "GB", Name = "United Kingdom" },
        new() { Id = 7, Code = "CA", Name = "Canada" },
        new() { Id = 8, Code = "JP", Name = "Japan" },
        new() { Id = 9, Code = "DE", Name = "Germany" },
        new() { Id = 10, Code = "FR", Name = "France" },
        new() { Id = 11, Code = "IN", Name = "India" },
        new() { Id = 12, Code = "CN", Name = "China" },
        new() { Id = 13, Code = "BR", Name = "Brazil" },
        new() { Id = 14, Code = "NO", Name = "Norway" },
        new() { Id = 15, Code = "NZ", Name = "New Zealand" },
        new() { Id = 16, Code = "TH", Name = "Thailand" },
        new() { Id = 17, Code = "KR", Name = "South Korea" },
        new() { Id = 18, Code = "IT", Name = "Italy" },
        new() { Id = 19, Code = "ES", Name = "Spain" },
        new() { Id = 20, Code = "EG", Name = "Egypt" },
    ];
}
