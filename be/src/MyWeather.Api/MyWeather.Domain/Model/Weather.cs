using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWeather.Domain.Model;

public class Weather
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("City")]
    public int CityId { get; set; }
    public City City { get; set; } = null!;
    public DateTime Date { get; set; } 
    public double WindSpeed { get; set; }
    public double WindDirection { get; set; }
    public double Temperature { get; set; }
    public double DewPoint { get; set; }
    public double Humidity { get; set; }
    public double Pressure { get; set; }
    public double Visibility { get; set; }
    public EnumCloudiness Cloudiness { get; set; }
}