using MyWeather.Domain.Common;

namespace MyWeather.Domain.Tests;

public class TemperatureConverterTests
{
    private static double ToFahrenheit(double celsius) =>
        TemperatureConverter.CelsiusToFahrenheit(celsius);

    [Fact]
    public void CelsiusToFahrenheit_AtFreezingPoint_Returns32()
    {
        Assert.Equal(32, ToFahrenheit(0), precision: 2);
    }

    [Fact]
    public void CelsiusToFahrenheit_AtBoilingPoint_Returns212()
    {
        Assert.Equal(212, ToFahrenheit(100), precision: 2);
    }

    [Theory]
    [InlineData(-1, 30.2)]
    [InlineData(-10, 14)]
    [InlineData(-40, -40)]
    [InlineData(-273.15, -459.67)]
    public void CelsiusToFahrenheit_WithNegativeCelsius_ReturnsExpectedFahrenheit(double celsius, double expectedFahrenheit)
    {
        Assert.Equal(expectedFahrenheit, ToFahrenheit(celsius), precision: 2);
    }

    [Theory]
    [InlineData(-0.5, 31.1)]
    [InlineData(0.5, 32.9)]
    [InlineData(99.5, 211.1)]
    [InlineData(36.6, 97.88)]
    public void CelsiusToFahrenheit_WithValuesAroundThresholds_ReturnsExpectedFahrenheit(double celsius, double expectedFahrenheit)
    {
        Assert.Equal(expectedFahrenheit, ToFahrenheit(celsius), precision: 2);
    }
}
