using System.ComponentModel.DataAnnotations;

namespace MyWeather.Domain.Model;

public class Country
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}