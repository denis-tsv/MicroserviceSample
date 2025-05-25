using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Weather.Infrastructure.Abstractions.WeatherServiceClient;

namespace Weather.Tests.Integration;

[CollectionDefinition(nameof(WeatherWebApiApplication))]
public class AstraApiApplicationCollectionFixture : ICollectionFixture<WeatherWebApiApplication>
{
}

public class WeatherWebApiApplication : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            //mock paid requests for tests
            var weatherServiceClient = Substitute.For<IWeatherServiceClient>();
            var random = new Random();
            var today = DateOnly.FromDateTime(DateTime.Today);
            weatherServiceClient.GetCurrentWeatherAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((float)random.NextDouble());
            weatherServiceClient.GetHistoricalWeatherAsync(Arg.Any<string>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
                .Returns([
                    new Historical(today, (float)random.NextDouble())
                ]);
            services.AddSingleton(weatherServiceClient);
        });
    }
}