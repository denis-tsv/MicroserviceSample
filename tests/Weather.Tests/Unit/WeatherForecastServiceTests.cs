using Weather.Domain.Services;

namespace Weather.Tests.Unit;

public class WeatherForecastServiceTests
{
    [Fact]
    public void Forecast()
    {
        //arrange
        var sut = new WeatherForecastService();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var data = new[]
        {
            new TemperatureDate(today.AddDays(-2), 10),
            new TemperatureDate(today.AddDays(-1), 20),
            new TemperatureDate(today.AddDays(0), 30)
        };

        //act
        var temperature = sut.Forecast(data, today.AddDays(1));
        
        //assert
        Assert.Equal(20, temperature);
    }
}
