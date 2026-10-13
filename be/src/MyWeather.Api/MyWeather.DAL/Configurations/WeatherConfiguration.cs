using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWeather.Domain.Model;

namespace MyWeather.DAL.Configurations;

public class WeatherConfiguration : IEntityTypeConfiguration<Weather>
{
    public void Configure(EntityTypeBuilder<Weather> builder)
    {
        builder.HasKey(w => w.Id);

        builder.HasOne(w => w.City)
            .WithMany()
            .HasForeignKey(w => w.CityId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(w => w.Cloudiness)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(w => new { w.CityId, w.Date });
    }
}
