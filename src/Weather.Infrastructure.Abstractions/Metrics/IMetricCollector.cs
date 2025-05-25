namespace Weather.Infrastructure.Abstractions.Metrics;

public interface IMetricCollector
{
    void ExternalWeatherServiceCalled();
}