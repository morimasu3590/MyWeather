using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWeather.Domain.Model;

public class City
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    [ForeignKey("Country")]
    public int CountryId { get; set; }
    public Country Country { get; set; } = null!;    
}