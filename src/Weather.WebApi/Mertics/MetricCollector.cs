using System.Diagnostics.Metrics;
using Weather.Infrastructure.Abstractions.Metrics;

namespace Weather.WebApi.Mertics;

public class MetricCollector : IMetricCollector, IDisposable
{
    public const string MeterName = "Weather";

    private readonly Meter _meter;
    private readonly Counter<long> _externalServiceCalledCount;
    
    public MetricCollector(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(MeterName, "1.0.0");

        _externalServiceCalledCount = _meter.CreateCounter<long>(name: "external.weather.service.called", description: "Counts calls of external weather service.");
    }

    public void Dispose()
    {
        _meter.Dispose();
    }

    public void ExternalWeatherServiceCalled()
    {
        _externalServiceCalledCount.Add(1);
    }
}