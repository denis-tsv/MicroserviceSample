using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Weather.Domain.Entities;
using Weather.Infrastructure.Abstractions.DataAccess;
using Weather.Infrastructure.Abstractions.WeatherServiceClient;

namespace Weather.Application.Services;

public interface IWeatherApplicationService
{
    Task<Measurement[]> GetMeasurementsAsync(string city, DateOnly dateFrom, DateOnly dateTo, CancellationToken cancellationToken);
}

public class WeatherApplicationService : IWeatherApplicationService
{
    private readonly IWeatherServiceClient _weatherServiceClient;
    private readonly IAppDbContext _dbContext;

    public WeatherApplicationService(
        IWeatherServiceClient weatherServiceClient,
        IAppDbContext dbContext)
    {
        _weatherServiceClient = weatherServiceClient;
        _dbContext = dbContext;
    }
    
    public async Task<Measurement[]> GetMeasurementsAsync(string city, DateOnly dateFrom, DateOnly dateTo, CancellationToken cancellationToken)
    {
        var measurements = await _dbContext.Measurements
            .AsNoTracking()
            .Where(x => x.City == city &&
                        x.Date >= dateFrom &&
                        x.Date <= dateTo)
            .ToArrayAsyncEF(cancellationToken);
        if (measurements.Length == dateTo.DayNumber - dateFrom.DayNumber + 1)
            return measurements;
        
        //each request is paid so it is cheaper to make one range request then many requests for each date 
        var historicalResponse = await _weatherServiceClient.GetHistoricalWeatherAsync(
            city, dateFrom, dateTo, cancellationToken);
        measurements = historicalResponse
            .Select(x => new Measurement
            {
                City = city,
                Date = x.Date,
                Temperature = x.Temperature
            }).ToArray();

        await _dbContext.Measurements.ToLinqToDB()
            .Merge()
            .Using(measurements)
            .On((entity, dto) => entity.City == dto.City && entity.Date == dto.Date)
            .InsertWhenNotMatched()
            .MergeAsync(cancellationToken);

        //id will be 0
        return measurements;
    }
}