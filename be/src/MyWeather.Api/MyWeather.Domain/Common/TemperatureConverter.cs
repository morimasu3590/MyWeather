namespace MyWeather.Domain.Common;

public static class TemperatureConverter
{
    public static double CelsiusToFahrenheit(double celsius) => celsius * 9 / 5 + 32;
}
