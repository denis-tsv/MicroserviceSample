using System.Net.Http.Json;
using Weather.Application.Commands;

namespace Weather.Tests.Integration;

[Collection(nameof(WeatherWebApiApplication))]
public class IntegrationTests
{
    private readonly WeatherWebApiApplication _application;
    private HttpClient _httpClient;

    public IntegrationTests(WeatherWebApiApplication application)
    {
        _application = application;
        _httpClient = application.CreateClient();
    }

    [Fact]
    public async Task GetCurrentWeather()
    {
        var response = await _httpClient.GetFromJsonAsync<GetCurrentWeatherResponseDto>("/weather/current?City=Tbilisi");
        Assert.NotNull(response);
    }
}