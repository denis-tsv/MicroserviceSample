using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Weather.Application.Services;
using Weather.Domain.Entities;
using Weather.Domain.Services;
using Weather.Infrastructure.Abstractions.DataAccess;

namespace Weather.Application.Commands;

public record GetForecastWeatherCommand(GetForecastWeatherRequestDto Dto) : IRequest<GetForecastWeatherResponseDto>;

public record GetForecastWeatherRequestDto(string City);
public record GetForecastWeatherResponseDto(float Temperature);

public class GetForecastWeatherRequestDtoValidator : AbstractValidator<GetForecastWeatherRequestDto>
{
    public GetForecastWeatherRequestDtoValidator()
    {
        RuleFor(x => x.City).NotEmpty();
    }
}

public class GetForecastWeatherCommandHandler : IRequestHandler<GetForecastWeatherCommand, GetForecastWeatherResponseDto>
{
    private readonly IWeatherApplicationService _weatherApplicationService;
    private readonly IWeatherForecastService _weatherForecastService;
    private readonly IAppDbContext _dbContext;

    public GetForecastWeatherCommandHandler(
        IWeatherApplicationService weatherApplicationService,
        IWeatherForecastService weatherForecastService,
        IAppDbContext dbContext)
    {
        _weatherApplicationService = weatherApplicationService;
        _weatherForecastService = weatherForecastService;
        _dbContext = dbContext;
    }
    public async Task<GetForecastWeatherResponseDto> Handle(GetForecastWeatherCommand request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var tomorrow = today.AddDays(1);
        var tomorrowMeasurement = await _dbContext.Measurements.AsNoTracking()
            .FirstOrDefaultAsync(x => x.City == request.Dto.City &&
                                      x.Date == tomorrow, cancellationToken);
        if (tomorrowMeasurement != null)
            return new GetForecastWeatherResponseDto(tomorrowMeasurement.Temperature);

        var lastWeekMeasurements = await _weatherApplicationService.GetMeasurementsAsync(request.Dto.City, today.AddDays(-7), today, cancellationToken);
        var temperatures = lastWeekMeasurements.Select(x => new TemperatureDate(x.Date, x.Temperature))
            .ToArray();
        var tomorrowTemperature = _weatherForecastService.Forecast(temperatures, tomorrow);
        tomorrowMeasurement = new Measurement
        {
            City = request.Dto.City,
            Date = tomorrow,
            Temperature = tomorrowTemperature
        };
        _dbContext.Measurements.Add(tomorrowMeasurement);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new GetForecastWeatherResponseDto(tomorrowTemperature);
    }
}