using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Weather.Infrastructure.Abstractions.Metrics;
using Weather.Infrastructure.Abstractions.WeatherServiceClient;

namespace Weather.Infrastructure.WeatherServiceClient;

public record CurrentWeatherResponse(Current Current);
public record Current(float Temperature);

public class WeatherStackServiceClient : IWeatherServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<WeatherStackConfiguration> _weatherStackConfiguration;
    private readonly IMetricCollector _metricCollector;

    public WeatherStackServiceClient(
        HttpClient httpClient, 
        IOptions<WeatherStackConfiguration> weatherStackConfiguration,
        IMetricCollector metricCollector)
    {
        _httpClient = httpClient;
        _weatherStackConfiguration = weatherStackConfiguration;
        _metricCollector = metricCollector;
    }

    public async Task<float> GetCurrentWeatherAsync(string city, CancellationToken cancellationToken)
    {
        _httpClient.BaseAddress = new Uri(_weatherStackConfiguration.Value.BaseUrl);
        var response = await _httpClient.GetFromJsonAsync<CurrentWeatherResponse>($"current?access_key={_weatherStackConfiguration.Value.ApiKey}&query={city}", cancellationToken);
        
        _metricCollector.ExternalWeatherServiceCalled();
        
        return response!.Current.Temperature;
    }

    public async Task<Historical[]> GetHistoricalWeatherAsync(string city, DateOnly dateFrom, DateOnly dateTo, CancellationToken cancellationToken)
    {
        var result = new List<Historical>(dateTo.DayNumber - dateFrom.DayNumber + 1);
        var random = new Random();
        for(var currentDate = dateFrom; currentDate <= dateTo; currentDate = currentDate.AddDays(1))
        {
            result.Add(new Historical(currentDate, random.Next(-30, 30)));
        }
        
        _metricCollector.ExternalWeatherServiceCalled();

        return result.ToArray();
        
        //request will fail because free license doesn't cover it 
        // _httpClient.BaseAddress = new Uri(_weatherStackConfiguration.Value.BaseUrl);
        // var response = await _httpClient.GetAsync($"historical?access_key={_weatherStackConfiguration.Value.ApiKey}&query={city}&historical_date_start={dateFrom}&historical_date_end={dateTo}", cancellationToken);
        // response.EnsureSuccessStatusCode();
        // var stringData = await response.Content.ReadAsStringAsync(cancellationToken);
        // var stringData = """
        //                  {
        //                     "historical": {
        //                         "2025-05-24": {
        //                             "hourly": [
        //                                 {
        //                                     "temperature": 27.3
        //                                 }
        //                             ]
        //                         }
        //                     }
        //                  }
        //                  """;
        // var document = JsonDocument.Parse(stringData);
        // return document.RootElement.GetProperty("historical")
        //     .EnumerateObject()
        //     .Select(x =>
        //     {
        //         var hourly = x.Value.GetProperty("hourly").EnumerateArray().First();
        //         var t = hourly.GetProperty("temperature").GetDouble();
        //         return new Historical(DateOnly.Parse(x.Name), (float) t);
        //     })
        //     .ToArray();
    }
}