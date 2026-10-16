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

        // Dates are stored and read back as UTC, so Kind is never Unspecified after a query.
        builder.Property(w => w.Date)
            .HasConversion(
                date => ToUtc(date),
                date => DateTime.SpecifyKind(date, DateTimeKind.Utc));

        builder.HasIndex(w => new { w.CityId, w.Date });
    }

    private static DateTime ToUtc(DateTime date) => date.Kind switch
    {
        DateTimeKind.Local => date.ToUniversalTime(),
        DateTimeKind.Unspecified => DateTime.SpecifyKind(date, DateTimeKind.Utc),
        _ => date
    };
}
