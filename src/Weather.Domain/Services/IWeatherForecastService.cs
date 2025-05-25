namespace Weather.Domain.Services;

public record TemperatureDate(DateOnly Date, float Temperature);

public interface IWeatherForecastService
{
    float Forecast(IReadOnlyCollection<TemperatureDate> measurements, DateOnly date);
}

public class WeatherForecastService : IWeatherForecastService
{
    public float Forecast(IReadOnlyCollection<TemperatureDate> measurements, DateOnly date)
    {
        //Newton interpolation will have more accuracy
        return measurements.Average(x => x.Temperature);
    }
}