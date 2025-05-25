namespace Weather.Infrastructure.Abstractions.WeatherServiceClient;

public record Historical(DateOnly Date, float Temperature);
public interface IWeatherServiceClient
{
    Task<float> GetCurrentWeatherAsync(string city, CancellationToken cancellationToken);
    Task<Historical[]> GetHistoricalWeatherAsync(string city, DateOnly dateFrom, DateOnly dateTo, CancellationToken cancellationToken);
}