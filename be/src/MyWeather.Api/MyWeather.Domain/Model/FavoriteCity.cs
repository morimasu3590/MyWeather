using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MyWeather.Domain.Model;

public class FavoriteCity
{
    [Key]
    public int Id { get; set; }
    public string Notes { get; set; } = string.Empty;
    [ForeignKey("City")]
    public int CityId { get; set; }
    public City City { get; set; } = null!;
}